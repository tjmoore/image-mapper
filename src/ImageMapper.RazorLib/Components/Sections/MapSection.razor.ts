import { statusContainerId } from './StatusSection.razor.js';
import { imageCountContainerId } from './ImageCountSection.razor.js';
import { progressContainerId } from './ProgressSection.razor.js';
import { triggerShowImage } from '../Overlays/ImageModal.razor.js';

/**
 * Represents the information of an image, including its file name, geographic coordinates, and URL.
 **/
export type ImageInfo = {
    id: string;
    fileName: string;
    filePath: string;
    url: string;
    width: number;
    height: number;
    latitude: number;
    longitude: number;    
};

declare const L: any;

let map: any;
let markerClusterGroup: any;
let markers: any[] = [];
let mapResizeHandlerAttached = false;
const popupTemplateId = 'map-popup-template';

/**
 * Initializes the Leaflet map and sets up the marker cluster group.
**/
export function initClusterMap(): void {
    map = L.map('map').setView([0, 0], 2);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    markerClusterGroup = L.markerClusterGroup();
    map.addLayer(markerClusterGroup);

    adjustMapLayout();

    if (!mapResizeHandlerAttached) {
        window.addEventListener('resize', adjustMapLayout);
        mapResizeHandlerAttached = true;
    }
}

/**
 * Adds a marker to the map for the given image data.
 **/
export function addMarkerToMap(imageData: ImageInfo): void {
    if (imageData.latitude && imageData.longitude) {
        const marker = L.marker([imageData.latitude, imageData.longitude]).bindPopup(createPopupPlaceholder());

        marker.on('popupopen', function (event: any): void {
            const popupElement = event.popup.getElement() as HTMLElement | null;
            if (!popupElement || popupElement.dataset.populated === 'true') {
                return;
            }

            const popupContent = popupElement.querySelector('.popup-content') as HTMLElement | null;
            if (!popupContent) {
                return;
            }

            populatePopupContent(popupContent, imageData);
            popupElement.dataset.populated = 'true';

            const fileLink = popupContent.querySelector('[data-popup-filename]') as HTMLElement | null;
            if (fileLink) {
                fileLink.addEventListener('click', function (): void {
                    triggerShowImage(imageData);
                });
            }

            const popupImage = popupContent.querySelector('.popup-full-image-trigger') as HTMLElement | null;
            if (popupImage) {
                popupImage.addEventListener('click', function (): void {
                    triggerShowImage(imageData);
                });
            }
        });

        markers.push(marker);
        markerClusterGroup.addLayer(marker);
    }
}

function createPopupPlaceholder(): HTMLElement {
    const templateRoot = document.getElementById(popupTemplateId);
    if (!templateRoot) {
        throw new Error(`Popup template element with id '${popupTemplateId}' was not found.`);
    }

    const popupRoot = templateRoot.cloneNode(true) as HTMLElement;
    popupRoot.removeAttribute('id');
    popupRoot.classList.remove('map-popup-template');
    popupRoot.dataset.populated = 'false';

    const popupImage = popupRoot.querySelector('[data-popup-image]') as HTMLImageElement | null;
    if (popupImage) {
        popupImage.removeAttribute('src');
        popupImage.removeAttribute('data-image-src');
    }

    return popupRoot;
}

function populatePopupContent(popupContent: HTMLElement, imageData: ImageInfo): void {
    const fileLink = popupContent.querySelector('[data-popup-filename]') as HTMLButtonElement | null;
    const popupImage = popupContent.querySelector('[data-popup-image]') as HTMLImageElement | null;

    if (!fileLink || !popupImage) {
        throw new Error('Popup template is missing required popup elements.');
    }

    fileLink.textContent = imageData.fileName;

    popupImage.src = imageData.url;
    popupImage.setAttribute('data-image-src', imageData.url);
    popupImage.title = `Click to view full-size image`;
}

/**
 * Adjusts the map layout based on the viewport size and the visibility of other UI components.
 **/
export function adjustMapLayout(): void {
    const mapElement = document.getElementById('map');
    if (!map || !mapElement) {
        return;
    }

    requestAnimationFrame(() => {
        const mapTop = mapElement.getBoundingClientRect().top;
        const viewportHeight = window.innerHeight;
        const bottomSpacing = 20;

        const containerIds = [statusContainerId, imageCountContainerId, progressContainerId];
        let visibleContainerHeight = 0;

        for (const id of containerIds) {
            const element = document.getElementById(id);
            if (!element) {
                continue;
            }

            const style = window.getComputedStyle(element);
            if (style.display === 'none') {
                continue;
            }

            const marginTop = parseFloat(style.marginTop || '0');
            const marginBottom = parseFloat(style.marginBottom || '0');
            visibleContainerHeight += element.offsetHeight + marginTop + marginBottom;
        }

        const availableHeight = viewportHeight - mapTop - visibleContainerHeight - bottomSpacing;
        mapElement.style.height = `${Math.max(220, availableHeight)}px`;
        map.invalidateSize();
    });
}

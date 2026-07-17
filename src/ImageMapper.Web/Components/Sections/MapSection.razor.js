import { statusContainerId } from './StatusSection.razor.js';
import { imageCountContainerId } from './ImageCountSection.razor.js';
import { progressContainerId } from './ProgressSection.razor.js';
import { showImageEventName } from '../Overlays/ImageModalSection.razor.js';

let map;
let markerClusterGroup;
let markers = [];
let mapResizeHandlerAttached = false;

export function initClusterMap() {
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

export function addMarkerToMap(imageData) {
    if (imageData.latitude && imageData.longitude) {
        const popupContent = `<div><strong>${imageData.fileName}</strong><br><img class="popup-thumb popup-thumb-image popup-full-image-trigger" src="${imageData.url}" data-image-src="${imageData.url}" title="Click to view full-size image"></div>`;

        const marker = L.marker([imageData.latitude, imageData.longitude]).bindPopup(popupContent);

        marker.on('popupopen', function (event) {
            const popupImage = event.popup.getElement()?.querySelector('.popup-full-image-trigger');
            if (popupImage) {
                popupImage.addEventListener('click', function () {
                    window.dispatchEvent(new CustomEvent(showImageEventName, {
                        detail: { imageSrc: imageData.url }
                    }));
                });
            }
        });

        markers.push(marker);
        markerClusterGroup.addLayer(marker);
    }
}

export function adjustMapLayout() {
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

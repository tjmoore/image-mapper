import { ImageInfo } from "../Sections/MapSection.razor.js";

let imageModalDotNetRef: any = null;
let keydownListener: ((event: KeyboardEvent) => void) | null = null;

export function setImageModalDotNetRef(dotNetRef: any): void {
    imageModalDotNetRef = dotNetRef;
}

export function triggerShowImage(imageInfo: ImageInfo): void {
    if (imageModalDotNetRef) {
        imageModalDotNetRef.invokeMethodAsync('ShowImage', imageInfo);
    }
}

export function setupImageModalKeyHandler(): void {
    if (keydownListener) {
        return;
    }

    keydownListener = function (event: KeyboardEvent): void {
        if (event.key === 'Escape' && imageModalDotNetRef) {
            imageModalDotNetRef.invokeMethodAsync('CloseModal');
        }
    };

    document.addEventListener('keydown', keydownListener);
}

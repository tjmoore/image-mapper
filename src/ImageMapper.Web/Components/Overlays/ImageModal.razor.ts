let imageModalDotNetRef: any = null;
let keydownListener: ((event: KeyboardEvent) => void) | null = null;

export function setImageModalDotNetRef(dotNetRef: any): void {
    imageModalDotNetRef = dotNetRef;
}

export function triggerShowImage(imageSrc: string): void {
    if (imageModalDotNetRef) {
        imageModalDotNetRef.invokeMethodAsync('ShowImage', imageSrc);
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

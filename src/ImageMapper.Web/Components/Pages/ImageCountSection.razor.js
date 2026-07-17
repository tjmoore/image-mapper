export const imageCountContainerId = 'imageCountContainer';
const layoutChangedEventName = 'image-mapper:layout-changed';

export function updateImageCount(count, skipped = 0) {
    const countValue = document.getElementById('imageCountValue');
    const countContainer = document.getElementById(imageCountContainerId);
    const skippedCountText = document.getElementById('skippedCountText');
    const skippedCountValue = document.getElementById('skippedCountValue');

    if (countValue) {
        countValue.textContent = count;
    }
    if (countContainer && count > 0) {
        countContainer.style.display = 'block';
    }

    if (skipped > 0) {
        if (skippedCountValue) {
            skippedCountValue.textContent = skipped;
        }
        if (skippedCountText) {
            skippedCountText.style.display = 'inline';
        }
    }

    window.dispatchEvent(new Event(layoutChangedEventName));
}

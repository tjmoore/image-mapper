export const progressContainerId = 'progressContainer';
const layoutChangedEventName = 'image-mapper:layout-changed';

export function showProgressContainer() {
    const container = document.getElementById(progressContainerId);
    if (container) {
        container.style.display = 'block';
        window.dispatchEvent(new Event(layoutChangedEventName));
    }
}

export function hideProgressContainer() {
    const container = document.getElementById(progressContainerId);
    if (container) {
        container.style.display = 'none';
        window.dispatchEvent(new Event(layoutChangedEventName));
    }
}

export function updateProgress(loaded, total, percentage) {
    const progressBar = document.getElementById('progressBar');
    const progressPercentage = document.getElementById('progressPercentage');
    const progressText = document.getElementById('progressText');

    if (progressBar) {
        progressBar.style.width = percentage + '%';
    }
    if (progressPercentage) {
        progressPercentage.textContent = percentage + '%';
    }
    if (progressText) {
        progressText.textContent = `Loading images... (${loaded}/${total})`;
    }
}

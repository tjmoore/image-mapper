export const progressContainerId = 'progressContainer';
export function setProgressBarWidth(percentage) {
    const progressBar = document.getElementById('progressBar');
    if (progressBar) {
        progressBar.style.width = percentage + '%';
    }
}

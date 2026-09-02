export const progressContainerId = 'progressContainer';
export function setProgressBarWidth(percentage: number): void {
    const progressBar = document.getElementById('progressBar');
    if (progressBar) {
        (progressBar as HTMLElement).style.width = `${percentage}%`;
    }
}

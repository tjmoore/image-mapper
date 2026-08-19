export const showImageEventName = 'image-mapper:show-image';

let modalHandlersAttached = false;

type ShowImageEventDetail = {
    imageSrc?: string;
};

type ShowImageEvent = Event & {
    detail?: ShowImageEventDetail;
};

export function setupImageModal(): void {
    if (modalHandlersAttached) {
        return;
    }

    const modal = document.getElementById('imageModal');
    const closeBtn = document.querySelector('.close') as HTMLElement | null;
    if (!modal || !closeBtn) {
        return;
    }

    closeBtn.onclick = function (): void {
        modal.classList.remove('show');
    };

    modal.onclick = function (event: MouseEvent): void {
        if (event.target === modal) {
            modal.classList.remove('show');
        }
    };

    document.addEventListener('keydown', function (event: KeyboardEvent): void {
        if (event.key === 'Escape' && modal.classList.contains('show')) {
            modal.classList.remove('show');
        }
    });

    window.addEventListener(showImageEventName, function (event: Event): void {
        const imageSrc = (event as ShowImageEvent).detail?.imageSrc;
        if (imageSrc) {
            showFullImage(imageSrc);
        }
    });

    modalHandlersAttached = true;
}

function showFullImage(imageSrc: string): void {
    const modal = document.getElementById('imageModal');
    const modalImage = document.getElementById('modalImage');
    if (!modal || !modalImage) {
        return;
    }

    (modalImage as HTMLImageElement).src = imageSrc;
    modal.classList.add('show');
}

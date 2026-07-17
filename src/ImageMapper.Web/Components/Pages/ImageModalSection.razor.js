export const showImageEventName = 'image-mapper:show-image';

let modalHandlersAttached = false;

export function setupImageModal() {
    if (modalHandlersAttached) {
        return;
    }

    const modal = document.getElementById('imageModal');
    const closeBtn = document.querySelector('.close');
    if (!modal || !closeBtn) {
        return;
    }

    closeBtn.onclick = function () {
        modal.classList.remove('show');
    };

    modal.onclick = function (event) {
        if (event.target === modal) {
            modal.classList.remove('show');
        }
    };

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && modal.classList.contains('show')) {
            modal.classList.remove('show');
        }
    });

    window.addEventListener(showImageEventName, function (event) {
        const imageSrc = event.detail?.imageSrc;
        if (imageSrc) {
            showFullImage(imageSrc);
        }
    });

    modalHandlersAttached = true;
}

function showFullImage(imageSrc) {
    const modal = document.getElementById('imageModal');
    const modalImage = document.getElementById('modalImage');
    if (!modal || !modalImage) {
        return;
    }

    modalImage.src = imageSrc;
    modal.classList.add('show');
}

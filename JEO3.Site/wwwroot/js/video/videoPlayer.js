export function initVideo(containerId, url, extensionType, marginTop) {
    const container = document.getElementById(containerId);
    if (!container || document.getElementById('myVideo')) return;

    const video = document.createElement('video');
    video.id = 'myVideo';
    video.autoplay = true;
    video.muted = true;
    video.playsInline = true;
    video.loop = true;

    // HTML5 Cache preloading directive:
    // 'auto' hints that the browser should download the entire video instantly
    video.preload = 'auto';

    video.style.width = '100%';
    video.style.height = '100%';
    video.style.objectFit = 'contain';
    video.style.marginTop = `${marginTop}px`;

    const source = document.createElement('source');
    source.src = url;
    source.type = 'video/' + extensionType;

    video.appendChild(source);
    container.appendChild(video);
}

export function changeVideo(url) {
    const video = document.getElementById('myVideo');
    if (video) {
        video.src = url;
        video.load(); // Forces the browser to discard the old cache stream and preload the new asset
        video.play().catch(err => console.log("Autoplay blocked or interrupted:", err));
    }
}
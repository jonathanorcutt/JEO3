

// Color Picker Set Site Theme Color
window.jeoThemeEngine = {
    setPrimaryColor: function (hexColor) {
        document.documentElement.style.setProperty('--rz-primary', hexColor);
        document.documentElement.style.setProperty('--jeo-primary', hexColor);
    }
};
// Monaco Editor Environment
window.MonacoEnvironment = {
    baseUrl: '_content/BlazorMonaco/lib/monaco-editor/min/vs'
};
// Monaco Editor Interop
window.monacoInterop = {
    defineAndSetTheme: function (themeName, themeData) {
        monaco.editor.defineTheme(themeName, themeData);
        monaco.editor.setTheme(themeName);
    }
};

// ---------------------------------------------------------------------------------

// Site Toggle Fullscreen/F11
async function toggleSiteFullscreen() {

    try {
        if (!document.fullscreenElement) {
            // Enter fullscreen
            await document.documentElement.requestFullscreen();
        } else {
            // Exit fullscreen
            await document.exitFullscreen();
        }

        // Fullscreen Hardcoded
        var style = 'height:100vh; width:100%; border:1px solid rgba(0,0,0,.08);';
        // document.querySelectorAll('.database-viewer-splitter').forEach((pane) => { pane.style = style + 'max-height: ' + ((!document.fullscreenElement) ? 785 : 930) + 'px'; });
        document.querySelectorAll('.navbar-row div').forEach((pane) => { pane.style.display = ((!document.fullscreenElement) ? 'block' : 'none'); });
    } catch (err) {
        console.error(`Error attempting to toggle fullscreen: ${err.message}`);
    }
};
// Toggle Diagram Pane Size To/From 50% Screen (Left Side)
async function toggleElementFullscreen(fullscreenDiv, enterFullScreen) {

    try {
        if (!fullscreenDiv) return;

        var requestMethod = fullscreenDiv.requestFullscreen ||
            fullscreenDiv.webkitRequestFullscreen ||
            fullscreenDiv.mozRequestFullScreen ||
            fullscreenDiv.msRequestFullscreen;

        if (requestMethod && enterFullScreen) {
            requestMethod.call(fullscreenDiv);
        } else {
            // Exit fullscreen
            await document.exitFullscreen();
        }
    } catch (err) {
        console.error(`Error attempting to toggle fullscreen: ${err.message}`);
    }
};
// Toggle Diagram Pane Size To/From 50% Screen (Left Side)
async function toggleElementHalfscreen(fullscreenDiv, enterFullScreen) {
    try {
        if (!fullscreenDiv) return;

        // Find the target TABLES titlebar element to read its current Y location
        const targetWorkspaceTablesContainer = document.querySelector('.dashboard-pane-titlebar-container');

        if (enterFullScreen) {
            // Default fallback if the TABLES titlebar isn't found in the DOM yet
            let targetTop = 50;

            if (targetWorkspaceTablesContainer) {
                // Get the exact pixel distance from the top of the browser viewport
                const rect = targetWorkspaceTablesContainer.getBoundingClientRect();
                targetTop = rect.top - 51.5;
            }

            // Lock it as a fixed overlay covering the left half of the screen stretching to the bottom
            fullscreenDiv.style.position = 'fixed';
            fullscreenDiv.style.top = `${targetTop}px`;
            fullscreenDiv.style.left = '8px'; // Snaps clean with 8px grid padding
            fullscreenDiv.style.width = fullscreenDiv.parentElement.style.flexBasis;//'56.3%'; // Matches left metadata column width perfectly
            fullscreenDiv.style.height = `calc(100vh - ${targetTop + 57}px)`;
            fullscreenDiv.style.zIndex = '99999'; // Layers it safely over the COLUMNS pane
            fullscreenDiv.classList.add('is-js-expanded');
        }
        else {
            // Revert all inline styles completely so it snaps back into its natural CSS Grid slot
            fullscreenDiv.style.position = '';
            fullscreenDiv.style.top = '';
            fullscreenDiv.style.left = '';
            fullscreenDiv.style.width = '';
            fullscreenDiv.style.height = '';
            fullscreenDiv.style.zIndex = '';
            fullscreenDiv.classList.remove('is-js-expanded');
        }

    } catch (err) {
        console.error(`Error attempting to toggle fullscreen: ${err.message}`);
    }
}

// ---------------------------------------------------------------------------------

// Download File - Base64
function downloadBase64File(contentType, base64String, fileName) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64String}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Download File - Stream
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();

    // FIX 1: Explicitly force "application/octet-stream" so the browser doesn't try to view it
    const blob = new Blob([arrayBuffer], { type: 'application/octet-stream' });

    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';

    // FIX 2: Temporarily attach to body (required by some browsers for the download attribute to work)
    document.body.appendChild(anchorElement);
    anchorElement.click();

    // Clean up
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

// ---------------------------------------------------------------------------------

// Pacman
function deleteNode(obj) {
    obj.deleteNode();
}

function updateCoordinates(e) {
    const container = e.currentTarget; // This is now your .monitor-panel
    const rect = container.getBoundingClientRect();

    // Calculate relative mouse positions within the panel frame
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    container.style.setProperty('--mouse-x', `${x}px`);
    container.style.setProperty('--mouse-y', `${y}px`);
}

// ---------------------------------------------------------------------------------

// Check if it already exists on the window to prevent redeclaration errors
if (!window.gridObserver) {
    window.gridObserver = new MutationObserver((mutations) => {
        for (const mutation of mutations) {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const target = mutation.target;

                // Check if this specific element just gained the active selection highlight class
                if (target && target.classList && target.classList.contains('rz-state-highlight')) {
                    target.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
            }
        }
    });
}

// Safely start or restart watching the layout tree
window.initializeGlobalGridScroller = () => {
    // ALWAYS fall back to document.body if the layout container isn't ready.
    // Document.body is never destroyed during Blazor page changes.
    const workspaceContainer = document.querySelector('.dashboard-full-absolute')
        || document.querySelector('.rz-layout')
        || document.body;

    if (workspaceContainer) {
        // Safe disconnect
        window.gridObserver.disconnect();

        window.gridObserver.observe(workspaceContainer, {
            attributes: true,
            subtree: true,
            attributeFilter: ['class']
        });
    }
};


// 🌟 THE ZOOM PROTECTION SHIELD
// Intercepts the mouse wheel event right at the browser window root level
window.addEventListener('wheel', function (e) {
    // 1. Check if the mouse cursor is currently hovering over your Blazor diagram canvases
    const target = e.target.closest('.sample-tile-layout .canvas-container');

    if (target) {
        // 2. Stop the browser window from moving up or down!
        e.preventDefault();

        // 3. Manually bubble or dispatch the wheel delta event straight to the diagram handler if needed, 
        // though Blazor Diagram's native wheel event listener will now catch it cleanly.
    }
}, { passive: false }); // 🌟 CRUCIAL: passive must be false so preventDefault() 
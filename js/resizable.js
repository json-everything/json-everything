window.initResizableSidebar = function () {
    const handles = document.querySelectorAll('.resize-handle');
    const sidebar = document.querySelector('.sidebar');
    const leftEditor = document.getElementById('left-editor-cell');
    const rightEditor = document.getElementById('right-editor-cell');
    const topEditorsRow = document.getElementById('top-editors-row');
    const bottomEditorCell = document.getElementById('bottom-editor-cell');
    const horizontalHandle = document.getElementById('horizontal-resize-handle');

    // --- Ratio state for editors ---
    let editorRatio = 0.5; // default 50/50 for vertical split
    let verticalRatio = 0.5; // default 50/50 for horizontal split

    // Helper to set editor widths by ratio
    function setEditorWidthsByRatio(ratio) {
        if (!leftEditor || !rightEditor) return;
        const parent = leftEditor.parentElement;
        if (!parent) return;
        const totalWidth = parent.clientWidth - handles[1].offsetWidth;
        const leftWidth = Math.round(totalWidth * ratio);
        const rightWidth = totalWidth - leftWidth;
        leftEditor.style.flex = `0 0 ${leftWidth}px`;
        rightEditor.style.flex = `0 0 ${rightWidth}px`;
    }

    // Helper to set editor heights by ratio
    function setEditorHeightsByRatio(ratio) {
        if (!topEditorsRow || !bottomEditorCell || !horizontalHandle) return;
        const parent = topEditorsRow.parentElement;
        if (!parent) return;
        const totalHeight = parent.clientHeight - horizontalHandle.offsetHeight;
        const topHeight = Math.round(totalHeight * ratio);
        topEditorsRow.style.flex = `0 0 ${topHeight}px`;
        bottomEditorCell.style.flex = `1 1 0`;
    }

    // --- Sidebar resize logic ---
    let isResizing = false;
    let startX;
    let startWidth;

    function startSidebarResize(e) {
        isResizing = true;
        startX = e.pageX;
        startWidth = parseInt(getComputedStyle(sidebar).width, 10);
        // Store current ratio before resizing
        if (leftEditor && rightEditor) {
            const leftW = leftEditor.getBoundingClientRect().width;
            const rightW = rightEditor.getBoundingClientRect().width;
            const total = leftW + rightW;
            editorRatio = total > 0 ? leftW / total : 0.5;
        }
        document.addEventListener('mousemove', handleSidebarMouseMove);
        document.addEventListener('mouseup', stopSidebarResize);
        e.preventDefault();
    }

    function handleSidebarMouseMove(e) {
        if (!isResizing) return;
        const width = startWidth + (e.pageX - startX);
        sidebar.style.width = `${width}px`;
        // After sidebar width changes, reapply the ratio to editors
        setEditorWidthsByRatio(editorRatio);
    }

    function stopSidebarResize() {
        isResizing = false;
        document.removeEventListener('mousemove', handleSidebarMouseMove);
        document.removeEventListener('mouseup', stopSidebarResize);
    }

    // --- Vertical handle for top editors ---
    let isVertResizing = false;
    let vertStartX;
    let leftStartWidth;
    let rightStartWidth;
    let totalStartWidth;

    function startVertResize(e) {
        isVertResizing = true;
        vertStartX = e.pageX;
        leftStartWidth = leftEditor.getBoundingClientRect().width;
        rightStartWidth = rightEditor.getBoundingClientRect().width;
        totalStartWidth = leftStartWidth + rightStartWidth;
        document.body.style.cursor = 'col-resize';
        document.addEventListener('mousemove', vertHandleMove);
        document.addEventListener('mouseup', vertHandleUp);
        e.preventDefault();
    }

    function vertHandleMove(e) {
        if (!isVertResizing) return;
        const dx = e.pageX - vertStartX;
        let newLeftWidth = leftStartWidth + dx;
        let newRightWidth = rightStartWidth - dx;
        // Prevent negative widths
        if (newLeftWidth < 50) newLeftWidth = 50;
        if (newRightWidth < 50) newRightWidth = 50;
        // Update ratio
        editorRatio = (newLeftWidth) / (newLeftWidth + newRightWidth);
        // Set flex-basis for both editors
        leftEditor.style.flex = `0 0 ${newLeftWidth}px`;
        rightEditor.style.flex = `0 0 ${newRightWidth}px`;
    }

    function vertHandleUp() {
        isVertResizing = false;
        document.body.style.cursor = '';
        document.removeEventListener('mousemove', vertHandleMove);
        document.removeEventListener('mouseup', vertHandleUp);
    }

    // --- Horizontal handle for top/bottom editors ---
    let isHorizResizing = false;
    let horizStartY;
    let topStartHeight;
    let bottomStartHeight;
    let totalStartHeight;

    function startHorizResize(e) {
        isHorizResizing = true;
        horizStartY = e.pageY;
        topStartHeight = topEditorsRow.getBoundingClientRect().height;
        bottomStartHeight = bottomEditorCell.getBoundingClientRect().height;
        totalStartHeight = topStartHeight + bottomStartHeight;
        document.body.style.cursor = 'row-resize';
        document.addEventListener('mousemove', horizHandleMove);
        document.addEventListener('mouseup', horizHandleUp);
        e.preventDefault();
    }

    function horizHandleMove(e) {
        if (!isHorizResizing) return;
        const dy = e.pageY - horizStartY;
        let newTopHeight = topStartHeight + dy;
        let newBottomHeight = bottomStartHeight - dy;
        // Prevent negative heights
        if (newTopHeight < 50) newTopHeight = 50;
        if (newBottomHeight < 50) newBottomHeight = 50;
        // Update ratio
        verticalRatio = (newTopHeight) / (newTopHeight + newBottomHeight);
        // Set flex-basis for top, let bottom fill remaining
        topEditorsRow.style.flex = `0 0 ${newTopHeight}px`;
        bottomEditorCell.style.flex = `1 1 0`;
    }

    function horizHandleUp() {
        isHorizResizing = false;
        document.body.style.cursor = '';
        document.removeEventListener('mousemove', horizHandleMove);
        document.removeEventListener('mouseup', horizHandleUp);
    }

    // Assign handlers to the three handles
    if (handles.length > 0) {
        // Sidebar handle (first)
        handles[0].addEventListener('mousedown', startSidebarResize);
        handles[0].addEventListener('touchstart', (e) => {
            startSidebarResize(e.touches[0]);
        });
        handles[0].addEventListener('touchmove', (e) => {
            handleSidebarMouseMove(e.touches[0]);
        });
        handles[0].addEventListener('touchend', stopSidebarResize);
    }
    if (handles.length > 1) {
        // Vertical handle (second)
        handles[1].addEventListener('mousedown', startVertResize);
        handles[1].addEventListener('touchstart', (e) => {
            startVertResize(e.touches[0]);
        });
        handles[1].addEventListener('touchmove', (e) => {
            vertHandleMove(e.touches[0]);
        });
        handles[1].addEventListener('touchend', vertHandleUp);
    }
    if (horizontalHandle) {
        horizontalHandle.addEventListener('mousedown', startHorizResize);
        horizontalHandle.addEventListener('touchstart', (e) => {
            startHorizResize(e.touches[0]);
        });
        horizontalHandle.addEventListener('touchmove', (e) => {
            horizHandleMove(e.touches[0]);
        });
        horizontalHandle.addEventListener('touchend', horizHandleUp);
    }

    // On window resize, maintain ratios
    window.addEventListener('resize', function () {
        setEditorWidthsByRatio(editorRatio);
        setEditorHeightsByRatio(verticalRatio);
    });

    // Initial set
    setTimeout(() => {
        setEditorWidthsByRatio(editorRatio);
        setEditorHeightsByRatio(verticalRatio);
    }, 0);
};

// Tooltip positioning
document.addEventListener('DOMContentLoaded', function() {
    document.addEventListener('mouseover', function(e) {
        const tooltipIcon = e.target.closest('.tooltip-icon');
        if (!tooltipIcon) return;
        
        const tooltip = tooltipIcon.querySelector('.tooltip-text');
        if (!tooltip) return;
        
        // Skip dynamic positioning for header tooltips - they use CSS positioning
        if (tooltip.classList.contains('header-tooltip')) return;
        
        const rect = tooltipIcon.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();
        
        // Position to the right of the icon
        let left = rect.right + 10;
        let top = rect.top;
        
        // Check if tooltip goes off the right edge
        if (left + tooltipRect.width > window.innerWidth) {
            left = rect.left - tooltipRect.width - 10; // Position to the left instead
        }
        
        // Check if tooltip goes off the bottom edge
        if (top + tooltipRect.height > window.innerHeight) {
            top = window.innerHeight - tooltipRect.height - 10;
        }
        
        // Check if tooltip goes off the top edge
        if (top < 0) {
            top = 10;
        }
        
        tooltip.style.left = left + 'px';
        tooltip.style.top = top + 'px';
    });
}); 
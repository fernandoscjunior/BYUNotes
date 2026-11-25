
window.mindMap = {
    initialize: function (svgId, dotNetHelper) {
        const svg = document.getElementById(svgId);

        if (!svg) return;

        if (svg.mindMapCleanup) {
            svg.mindMapCleanup();
        }

        let activeNode = null;
        let activePointer = null;
        let startPoint = null;
        let originalX = 0;
        let originalY = 0;
        let moved = false;
        let suppressClick = false;
        let saving = false;

        function getPoint(event) {
            const point = svg.createSVGPoint();

            point.x = event.clientX;
            point.y = event.clientY;

            return point.matrixTransform(
                svg.getScreenCTM().inverse()
            );
        }

        function pointerDown(event) {
            if (event.button !== 0 || saving || activeNode) return;

            const node = event.target.closest("[data-node-id]");

            if (!node || !svg.contains(node)) return;

            activeNode = node;
            activePointer = event.pointerId;
            startPoint = getPoint(event);

            originalX = Number(node.getAttribute("x"));
            originalY = Number(node.getAttribute("y"));

            moved = false;
            suppressClick = false;
        }

        function updateLines(id, centerX, centerY) {
            svg.querySelectorAll("line[data-child-id]")
                .forEach(line => {
                    if (line.dataset.childId === id) {
                        line.setAttribute("x2", centerX);
                        line.setAttribute("y2", centerY);
                    }

                    if (line.dataset.parentId === id) {
                        line.setAttribute("x1", centerX);
                        line.setAttribute("y1", centerY);
                    }
                });
        }

        function pointerMove(event) {
            if (!activeNode ||
                event.pointerId !== activePointer) return;

            const point = getPoint(event);

            const dx = point.x - startPoint.x;
            const dy = point.y - startPoint.y;

            if (!moved && Math.hypot(dx, dy) < 5) {
                return;
            }

            if (!moved) {
                svg.setPointerCapture(event.pointerId);
            }

            moved = true;
            suppressClick = true;

            event.preventDefault();

            const x = Math.max(0, originalX + dx);
            const y = Math.max(0, originalY + dy);

            activeNode.setAttribute("x", x);
            activeNode.setAttribute("y", y);

            activeNode.style.cursor = "grabbing";

            const centerX =
                x + Number(activeNode.getAttribute("width")) / 2;

            const centerY =
                y + Number(activeNode.getAttribute("height")) / 2;

            updateLines(
                activeNode.dataset.nodeId,
                centerX,
                centerY
            );
        }

        async function pointerUp(event) {
            if (!activeNode ||
                event.pointerId !== activePointer) return;

            const node = activeNode;
            const wasMoved = moved;

            activeNode = null;
            activePointer = null;
            moved = false;

            node.style.cursor = "grab";

            if (svg.hasPointerCapture(event.pointerId)) {
                svg.releasePointerCapture(event.pointerId);
            }

            if (!wasMoved) return;

            const id = node.dataset.nodeId;

            const x = Number(node.getAttribute("x"))
                + Number(node.getAttribute("width")) / 2;

            const y = Number(node.getAttribute("y"))
                + Number(node.getAttribute("height")) / 2;

            saving = true;

            console.log("Saving mind map position:", id, x, y);

            try {
                await dotNetHelper.invokeMethodAsync(
                    "SaveNodePosition",
                    id,
                    x,
                    y
                );

                console.log("Position callback completed:", id);
            } catch (error) {
                console.error("Unable to save position:", error);
            } finally {
                saving = false;
            }
        }

        function preventDragClick(event) {
            if (suppressClick) {
                event.preventDefault();
                event.stopImmediatePropagation();

                suppressClick = false;
            }
        }

        svg.addEventListener("pointerdown", pointerDown);

        window.addEventListener("pointermove", pointerMove);
        window.addEventListener("pointerup", pointerUp);
        window.addEventListener("pointercancel", pointerUp);

        svg.addEventListener("click", preventDragClick, true);

        svg.mindMapCleanup = function () {
            svg.removeEventListener("pointerdown", pointerDown);

            window.removeEventListener("pointermove", pointerMove);
            window.removeEventListener("pointerup", pointerUp);
            window.removeEventListener("pointercancel", pointerUp);

            svg.removeEventListener("click", preventDragClick, true);
        };

        console.log("Mind map initialized successfully.");
    },

    dispose: function (svgId) {
        const svg = document.getElementById(svgId);

        if (svg && svg.mindMapCleanup) {
            svg.mindMapCleanup();
        }
    }
};

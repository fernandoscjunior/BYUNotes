
window.noteExport = {

    download: async function (elementId, fileName, format) {

        const element = document.getElementById(elementId);

        if (!element) {
            throw new Error("Note export preview not found.");
        }

        await document.fonts.ready;

        // Create an independent copy of the export layout.
        const clone = element.cloneNode(true);

        const originals = [
            element,
            ...element.querySelectorAll("*")
        ];

        const copies = [
            clone,
            ...clone.querySelectorAll("*")
        ];

        // Preserve the original formatting.
        originals.forEach((original, index) => {

            const styles = window.getComputedStyle(original);
            const target = copies[index];

            for (const property of styles) {
                target.style.setProperty(
                    property,
                    styles.getPropertyValue(property)
                );
            }
        });

        const width = element.scrollWidth;

        clone.removeAttribute("id");

        clone.style.position = "relative";
        clone.style.left = "0";
        clone.style.top = "0";
        clone.style.margin = "0";
        clone.style.width = width + "px";
        clone.style.maxWidth = "none";
        clone.style.height = "auto";
        clone.style.overflow = "visible";
        clone.style.backgroundColor = "#ffffff";

        // HTML EXPORT
        if (format === "html") {

            const doc = document.implementation
                .createHTMLDocument(fileName);

            const charset = doc.createElement("meta");
            charset.setAttribute("charset", "utf-8");
            doc.head.prepend(charset);

            doc.body.style.margin = "30px";
            doc.body.style.background = "#ffffff";

            doc.body.appendChild(clone);

            const html = "<!DOCTYPE html>\n" +
                doc.documentElement.outerHTML;

            this.saveFile(
                html,
                fileName + ".html",
                "text/html;charset=utf-8"
            );

            return;
        }

        // Verify image rendering library.
        if (typeof html2canvas === "undefined") {
            throw new Error("html2canvas is not loaded.");
        }

        // Temporary container for visual exports.
        const container = document.createElement("div");

        container.style.position = "absolute";
        container.style.left = "0";
        container.style.top = "0";
        container.style.width = width + "px";
        container.style.background = "#ffffff";
        container.style.pointerEvents = "none";
        container.style.zIndex = "-9999";

        container.appendChild(clone);
        document.body.appendChild(container);

        try {

            await new Promise(resolve =>
                requestAnimationFrame(resolve)
            );

            const captureWidth = clone.scrollWidth;
            const captureHeight = clone.scrollHeight;

            // Limit canvas size for very large notes.
            const scale = Math.min(
                2,
                16000 / captureWidth,
                16000 / captureHeight,
                Math.sqrt(
                    32000000 /
                    (captureWidth * captureHeight)
                )
            );

            const canvas = await html2canvas(clone, {

                scale: scale,
                backgroundColor: "#ffffff",
                useCORS: true,
                logging: false,

                width: captureWidth,
                height: captureHeight,

                windowWidth: Math.max(
                    window.innerWidth,
                    captureWidth
                ),

                windowHeight: Math.max(
                    window.innerHeight,
                    captureHeight
                ),

                scrollX: 0,
                scrollY: 0
            });

            // PNG EXPORT
            if (format === "png") {

                const blob = await new Promise(resolve =>
                    canvas.toBlob(resolve, "image/png")
                );

                if (!blob) {
                    throw new Error("Unable to create PNG.");
                }

                this.saveFile(
                    blob,
                    fileName + ".png",
                    "image/png"
                );

                return;
            }

            // PDF EXPORT
            if (format === "pdf") {

                if (!window.jspdf) {
                    throw new Error("jsPDF is not loaded.");
                }

                const { jsPDF } = window.jspdf;

                const isMappingNote =
                    element.textContent.includes(
                        "BYUNOTES | MAPPING NOTES"
                    );

                const pdf = new jsPDF({

                    orientation: isMappingNote
                        ? "landscape"
                        : "portrait",

                    unit: "mm",
                    format: "a4"
                });

                const margin = 10;

                const pageWidth =
                    pdf.internal.pageSize.getWidth()
                    - margin * 2;

                const pageHeight =
                    pdf.internal.pageSize.getHeight()
                    - margin * 2;

                const pixelsPerMillimeter =
                    canvas.width / pageWidth;

                const pageHeightPixels = Math.max(
                    1,
                    Math.floor(
                        pageHeight * pixelsPerMillimeter
                    )
                );

                let position = 0;
                let pageNumber = 0;

                // Split long notes across multiple pages.
                while (position < canvas.height) {

                    const sliceHeight = Math.min(
                        pageHeightPixels,
                        canvas.height - position
                    );

                    const pageCanvas =
                        document.createElement("canvas");

                    pageCanvas.width = canvas.width;
                    pageCanvas.height = sliceHeight;

                    const context =
                        pageCanvas.getContext("2d");

                    context.fillStyle = "#ffffff";

                    context.fillRect(
                        0,
                        0,
                        pageCanvas.width,
                        pageCanvas.height
                    );

                    context.drawImage(
                        canvas,
                        0,
                        position,
                        canvas.width,
                        sliceHeight,
                        0,
                        0,
                        canvas.width,
                        sliceHeight
                    );

                    if (pageNumber > 0) {
                        pdf.addPage();
                    }

                    const imageHeight =
                        sliceHeight / pixelsPerMillimeter;

                    pdf.addImage(
                        pageCanvas.toDataURL("image/png"),
                        "PNG",
                        margin,
                        margin,
                        pageWidth,
                        imageHeight
                    );

                    // Page number
                    pdf.setFontSize(9);
                    pdf.setTextColor(100, 116, 139);

                    pdf.text(
                        `Page ${pageNumber + 1}`,
                        pdf.internal.pageSize.getWidth() / 2,
                        pdf.internal.pageSize.getHeight() - 4,
                        { align: "center" }
                    );

                    position += sliceHeight;
                    pageNumber++;
                }

                pdf.save(fileName + ".pdf");

                return;
            }

            throw new Error(
                "Unsupported export format: " + format
            );

        } finally {

            // Always remove the temporary export container.
            container.remove();
        }
    },

    saveFile: function (content, fileName, contentType) {

        const blob = content instanceof Blob
            ? content
            : new Blob([content], {
                type: contentType
            });

        const url = URL.createObjectURL(blob);

        const link = document.createElement("a");

        link.href = url;
        link.download = fileName;

        document.body.appendChild(link);

        link.click();
        link.remove();

        setTimeout(
            () => URL.revokeObjectURL(url),
            1000
        );
    }
};


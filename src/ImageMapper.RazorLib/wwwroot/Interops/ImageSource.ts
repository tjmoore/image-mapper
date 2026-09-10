export async function setImageSource(elementId: string, streamDotNetRef: any, contentType: string, title: string): Promise<void> {
    const arrayBuffer = await streamDotNetRef.arrayBuffer();
    let blobOptions: BlobPropertyBag = {};
    if (contentType) {
        blobOptions['type'] = contentType;
    }
    const blob = new Blob([arrayBuffer], blobOptions);
    const url = URL.createObjectURL(blob);
    const element = document.getElementById(elementId) as HTMLImageElement | null;
    if (element) {
        element.title = title;
        element.onload = () => {
            URL.revokeObjectURL(url);
        }
        element.src = url;
    }
}
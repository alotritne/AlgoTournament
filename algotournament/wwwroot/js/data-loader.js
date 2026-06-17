export async function fetchMockData(fileName) {
    try {
        const response = await fetch(`../data/${fileName}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Failed to load ${fileName}:`, error);
        return null;
    }
}

/**
 * Device this page runs on, for the configuration user test: each survivor is configured on a
 * different device, and the game logs the web's grabs in `webAppTestData.json` under it
 * (`"Computer"`, `"Tablet"`, `"Phone"`). Same values as the server's `TEST_DEVICES`.
 */
export type TestDevice = "computer" | "tablet" | "phone";

const DEVICES: readonly TestDevice[] = ["computer", "tablet", "phone"];

/** Smallest screen side, in CSS pixels, from which a touch device counts as a tablet. */
const TABLET_MIN_SIDE = 600;

/**
 * `?device=computer|tablet|phone` in the page URL when present, which is the reliable way: open
 * each device once with its own link. Without it the device is guessed: a mouse is a computer,
 * and a touch screen is a phone or a tablet by the size of its smallest side.
 */
function detectDevice(): TestDevice {
    const param = new URLSearchParams(location.search).get("device")?.trim().toLowerCase();

    if (param && (DEVICES as readonly string[]).includes(param)) {
        return param as TestDevice;
    }

    if (!matchMedia("(pointer: coarse)").matches) {
        return "computer";
    }

    return Math.min(screen.width, screen.height) < TABLET_MIN_SIDE ? "phone" : "tablet";
}

/** Read once: the device does not change while the page is open. */
export const TEST_DEVICE: TestDevice = detectDevice();

console.log(`Dispositivo do teste: ${TEST_DEVICE}`);

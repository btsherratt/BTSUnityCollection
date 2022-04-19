using UnityEngine;

public static class ColorExtensions {
    public static Color FromHex(uint hexValue) {
        uint r = (hexValue >> 16) & 255;
        uint g = (hexValue >> 08) & 255;
        uint b = (hexValue >> 00) & 255;

        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
    }
}

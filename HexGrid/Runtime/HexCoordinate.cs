using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HexCoordinate : IEquatable<HexCoordinate> {
    public static HexCoordinate Zero = new HexCoordinate(0, 0, 0);

    public enum Corner {
        N,
        NW,
        SW,
        S,
        SE,
        NE,
    }

    public enum Direction {

    }

    public static HexCoordinate operator +(HexCoordinate lhs, HexCoordinate rhs) {
        return new HexCoordinate(lhs.q + rhs.q, lhs.r + rhs.r, lhs.s + rhs.s);
    }

    public static HexCoordinate operator -(HexCoordinate lhs, HexCoordinate rhs) {
        return new HexCoordinate(lhs.q - rhs.q, lhs.r - rhs.r, lhs.s - rhs.s);
    }

    public static HexCoordinate operator *(HexCoordinate lhs, int rhs) {
        return new HexCoordinate(lhs.q * rhs, lhs.r * rhs, lhs.s * rhs);
    }

    public static bool operator ==(HexCoordinate lhs, HexCoordinate rhs) {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(HexCoordinate lhs, HexCoordinate rhs) {
        return !(lhs == rhs);
    }

    public static HexCoordinate[] ms_directions = { // FIXME
        new HexCoordinate( 1,  0, -1),
        new HexCoordinate( 1, -1,  0),
        new HexCoordinate( 0, -1,  1),
        new HexCoordinate(-1,  0,  1),
        new HexCoordinate(-1,  1,  0),
        new HexCoordinate( 0,  1, -1),
    };

    public int q;
    public int r;
    public int s;

    public HexCoordinate(int q, int r, int s) {
        this.q = q;
        this.r = r;
        this.s = s;
    }

    public override bool Equals(object obj) {
        return obj is HexCoordinate other && this.Equals(other);
    }

    public bool Equals(HexCoordinate p) {
        return q == p.q &&
            r == p.r &&
            s == p.s;
    }

    public override int GetHashCode() {
        return q ^ r ^ s;
    }
}

public static class HexCoordinateExtensions {
    public static IEnumerable<HexCoordinate> Ring(this HexCoordinate coordinate, int radius) {
        HexCoordinate offset = HexCoordinate.ms_directions[4] * radius;

        for (int i = 0; i < 6; ++i) {
            for (int j = 0; j < radius; ++j) {
                HexCoordinate offsetCoordinate = coordinate + offset;
                yield return offsetCoordinate;
                offset += HexCoordinate.ms_directions[i];
            }
        }
    }
    public static IEnumerable<Vector3> EdgeRing(this HexCoordinate centreCoordinate, int radius, float size = 1.0f) {
        IEnumerator<HexCoordinate> hexCoodinateEnumerator = centreCoordinate.Ring(radius).GetEnumerator();

        for (int j = 0; j < radius; ++j) {
            hexCoodinateEnumerator.MoveNext();
            HexCoordinate coordinate = hexCoodinateEnumerator.Current;
            yield return coordinate.Corner(HexCoordinate.Corner.NW, size);
            yield return coordinate.Corner(HexCoordinate.Corner.N, size);
        }

        yield return hexCoodinateEnumerator.Current.Corner(HexCoordinate.Corner.NE, size);

        for (int j = 0; j < radius; ++j) {
            hexCoodinateEnumerator.MoveNext();
            HexCoordinate coordinate = hexCoodinateEnumerator.Current;
            yield return coordinate.Corner(HexCoordinate.Corner.N, size);
            yield return coordinate.Corner(HexCoordinate.Corner.NE, size);
        }

        yield return hexCoodinateEnumerator.Current.Corner(HexCoordinate.Corner.SE, size);

        for (int j = 0; j < radius; ++j) {
            hexCoodinateEnumerator.MoveNext();
            HexCoordinate coordinate = hexCoodinateEnumerator.Current;
            yield return coordinate.Corner(HexCoordinate.Corner.NE, size);
            yield return coordinate.Corner(HexCoordinate.Corner.SE, size);
        }

        yield return hexCoodinateEnumerator.Current.Corner(HexCoordinate.Corner.S, size);
        hexCoodinateEnumerator.MoveNext();
        yield return hexCoodinateEnumerator.Current.Corner(HexCoordinate.Corner.SE, size);

        for (int j = 0; j < radius; ++j) {
            HexCoordinate coordinate = hexCoodinateEnumerator.Current;
            yield return coordinate.Corner(HexCoordinate.Corner.S, size);
            yield return coordinate.Corner(HexCoordinate.Corner.SW, size);
            hexCoodinateEnumerator.MoveNext();
        }

        yield return hexCoodinateEnumerator.Current.Corner(HexCoordinate.Corner.S, size);

        for (int j = 0; j < radius; ++j) {
            HexCoordinate coordinate = hexCoodinateEnumerator.Current;
            yield return coordinate.Corner(HexCoordinate.Corner.SW, size);
            yield return coordinate.Corner(HexCoordinate.Corner.NW, size);
            hexCoodinateEnumerator.MoveNext();
        }

        yield return hexCoodinateEnumerator.Current.Corner(HexCoordinate.Corner.SW, size);

        for (int j = 0; j < radius; ++j) {
            HexCoordinate coordinate = hexCoodinateEnumerator.Current;
            yield return coordinate.Corner(HexCoordinate.Corner.NW, size);
            yield return coordinate.Corner(HexCoordinate.Corner.N, size);
            hexCoodinateEnumerator.MoveNext();
        }
    }

    public static Vector3 Position(this HexCoordinate coordinate, float size = 1.0f) {
        Vector3 position = new Vector3() {
            x = (Mathf.Sqrt(3.0f) * coordinate.q + Mathf.Sqrt(3.0f) / 2.0f * coordinate.r) * size,
            y = (3.0f / 2.0f * coordinate.r) * size,
            z = 0.0f,
        };
        return position;
    }

    public static Vector3 Corner(this HexCoordinate coordinate, HexCoordinate.Corner corner, float size = 1.0f) {
        return Position(coordinate, size) + (Quaternion.Euler(0, 0, (int)corner * 60) * Vector3.up * size);
    }

    public static HexCoordinate ToCoordinate(this Vector3 position, float size = 1.0f) {
        HexCoordinate coordinate = new HexCoordinate() {
            q = Mathf.RoundToInt((Mathf.Sqrt(3.0f) / 3.0f * position.x - 1.0f / 3.0f * position.y) / size),
            r = Mathf.RoundToInt((2.0f / 3.0f * position.y) / size),
            s = 0,
        };
        coordinate.s = -coordinate.q - coordinate.r;
        return coordinate;
    }
}
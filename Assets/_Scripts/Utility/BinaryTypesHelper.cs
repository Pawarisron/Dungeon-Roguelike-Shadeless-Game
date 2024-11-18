using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


//  Summary:
//      a helper of SimplifyFloor to reduce protrutions from the floor to make the map more open.
public static class SimplifyFloorHelper
{
    public static HashSet<byte> reduceSet = new HashSet<byte>
    {
        //# = floor
        //X = walls

        //# # #
        //# X #
        //# # #
        0b11111111,
        0b10111111,
        0b11101111,
        0b11111011,
        0b11111110,

        //X # #
        //X X #
        //X # #
        0b00111110,
        0b11111000,
        0b11100011,
        0b10001111
        
        
    };


}



//  Summary: 
//      WallTypesHelper is a helper of WallGenerator to find tiles to place properly
//      Using Direction2D to check every diagonalDirections and eightDirections list
//      1 = floor
//      0 = not floor
public static class WallTypesHelper
{
    
    public static HashSet<byte> wallTop = new HashSet<byte>
    {
        0b1111,
        0b0110,
        0b0011,
        0b0010,
        0b1010,
        0b1100,
        0b1110,
        0b1011,
        0b0111
    };

    public static HashSet<byte> wallSideLeft = new HashSet<byte>
    {
        0b0100
    };

    public static HashSet<byte> wallSideRight = new HashSet<byte>
    {
        0b0001
    };

    public static HashSet<byte> wallBottom = new HashSet<byte>
    {
        0b1000
    };

    public static HashSet<byte> wallInnerCornerDownLeft = new HashSet<byte>
    {
        0b11110001,
        0b11100000,
        0b11110000,
        0b11100001,
        0b10100000,
        0b01010001,
        0b11010001,
        0b01100001,
        0b11010000,
        0b01110001,
        0b00010001,
        0b10110001,
        0b10100001,
        0b10010000,
        0b00110001,
        0b10110000,
        0b00100001,
        0b10010001
    };

    public static HashSet<byte> wallInnerCornerDownRight = new HashSet<byte>
    {
        0b11000111,
        0b11000011,
        0b10000011,
        0b10000111,
        0b10000010,
        0b01000101,
        0b11000101,
        0b01000011,
        0b10000101,
        0b01000111,
        0b01000100,
        0b11000110,
        0b11000010,
        0b10000100,
        0b01000110,
        0b10000110,
        0b11000100,
        0b01000010

    };

    public static HashSet<byte> wallDiagonalCornerDownLeft = new HashSet<byte>
    {
        0b01000000
    };

    public static HashSet<byte> wallDiagonalCornerDownRight = new HashSet<byte>
    {
        0b00000001
    };

    public static HashSet<byte> wallDiagonalCornerUpLeft = new HashSet<byte>
    {
        0b00010000,
        0b01010000,
    };

    public static HashSet<byte> wallDiagonalCornerUpRight = new HashSet<byte>
    {
        0b00000100,
        0b00000101
    };

    public static HashSet<byte> wallFull = new HashSet<byte>
    {
        0b1101,
        0b0101,
        0b1101,
        0b1001

    };

    public static HashSet<byte> wallFullEightDirections = new HashSet<byte>
    {
        0b00010100,
        0b11100100,
        0b10010011,
        0b01110100,
        0b00010111,
        0b00010110,
        0b00110100,
        0b00010101,
        0b01010100,
        0b00010010,
        0b00100100,
        0b00010011,
        0b01100100,
        0b10010111,
        0b11110100,
        0b10010110,
        0b10110100,
        0b11100101,
        0b11010011,
        0b11110101,
        0b11010111,
        0b11010111,
        0b11110101,
        0b01110101,
        0b01010111,
        0b01100101,
        0b01010011,
        0b01010010,
        0b00100101,
        0b00110101,
        0b01010110,
        0b11010101,
        0b11010100,
        0b10010101

    };

    public static HashSet<byte> wallBottmEightDirections = new HashSet<byte>
    {
        0b01000001
    };

}

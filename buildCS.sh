#!/bin/bash
rm test.wav
rm TVmod.exe
mcs /reference:System.Drawing.dll TVmod.cs
mono TVmod.exe

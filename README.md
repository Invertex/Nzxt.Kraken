# Nzxt.Kraken

A simple program to access the temperature, lighting, fan and pump speed of the NZXT Kraken X62.
Because fuck CAM.

Use the core library and Manager class or the launcher;

Eg; `Nzxt.Kraken.Controller.exe /r <red> /g <green> /b <blue> /fan <fan-speed> /pump <pump-speed> /monitor`

* red/green/blue are RGB values for the lighting color (all three must be specified, /r 255 /g 0 /b 0 is red).
* fan-speed/pump-speed are percent 0 to 100 (the minimum seems to be %60).
* monitor if specified the program will loop outputting information.

All options are optional.

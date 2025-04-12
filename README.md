# DL Link

## Overview
The DL Link plugin is a device driver for N.I.N.A. that allows you to control Digital Logger outlets. It provides functionality to turn-on, tunr-off and cycle outlets in the switch equipment section as well as in sequences.
It also provides functionality to refresh the list of available devices in NINA after a switch action. The Seqence items work regardless whether you use the switch driver or have another switch connected.

Use this to power on a switch like the Pegasus Astro power boxes, then refresh and connect to that switch. Now you can turn on the power to your camera, the USB port for the guide camera etc. Then use the refresh actions to find all the new devices and you can use the Connector plugin (separate plugin) to connnect to the newfound hardware.

Please note that the password used for DL switches is transmitted in the clear, so we are also not encrypting it in the plugin itself.

## Installation
Installation
To install the DL Link plugin:
 - Download the plugin from the official repository.</li>
 - Or place the plugin files manually in the N.I.N.A. plugins directory.</li>
 - Restart N.I.N.A. to load the plugin.</li>

## Usage
To use the DL Link plugin:
 - Navigate to the equipment settings in N.I.N.A.</li>
 - Select the DL Link plugin from the list of installed plugins.</li>
 - Configure the server address, username, and password in the plugin settings. Note that the password will not be ecnrypted, as DL switches use digest auth over http.</li>
 - Connect to the device and manage outlets as needed.</li>

## Issues
You can submit <a href="https://github.com/ivonnyssen/nina-dl-link/issues">issues</a> or contribute <a href="https://github.com/ivonnyssen/nina-dl-link">code</a> on here.

## License
This plugin is licensed under the MIT License. See the LICENSE file for more information.

## Screenshots

### Plugin Options

![DL Link Plugin](https://raw.githubusercontent.com/ivonnyssen/nina-dl-link/main/docs/assets/DL-Link-Options.png)

### Sequence Example

![DL Link Plugin](https://raw.githubusercontent.com/ivonnyssen/nina-dl-link/main/docs/assets/DL-Link-Sequence.png)

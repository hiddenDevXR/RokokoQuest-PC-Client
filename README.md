# Rokoko Quest - PC Client

## About

This is the client side app for PC. This app pupose is to serve as a bridge between the Rokoko offical app and the VRL Theathers Quest app.
The repository includes the Rokoko libraries to read the motion capture data and the Photon libraires using for networking. The app uses the Rokoko library to translate motion capture data to a humanoid skeletal mesh. After that, the mocap info curated for the current active skeletal mesh model is send to oculus Quest using the Photon PUN services. The Quest receives the data and apply it to their side skeletal mesh. In this way, the user on the Quest can see in real time the dance performace of the artist in the mocap suit.

This project is meant to run from the editor and not as a stand alone exe and should be of considerationt that is not the final version and can be greatly improved.

![Alt text](https://github.com/hiddenDevXR/rokokoQuest/blob/main/resources/rokoko1.PNG)

## Aknowledgment

This projects is part of the Viral Theaters Exhibition and was made possible thanks to

Humbolft University of Berlin
https://tieranatomisches-theater.de/en/project/2022-viral-theatres_en/

Gamelab Berlin

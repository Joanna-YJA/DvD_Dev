Things to do:

*Configure the other receiver with Gerard first

Screenshot:
v Virtual world with map at pre-determined location. Face CMPB to give some context
v Drone video feed
- Drone video feed with overlay of enemy Drone
- Drone video feed with overlay of directional indicator
v - Drone video feed with box up of enemy drone

Video:
- Drone video feed with box up --> Panning around
- Can try to record the virtual feed simultaneously to show spatial awareness


Handing over:
Dependencies
Java:
- Copy out Eclipse Mars 2 in C drive
- Install Java 8 (jdk 8u131)
- Set 192.168.XXX.XXX in iPV4, Advanced Settings --> This is for Luciad to run
- Install Microsoft Loopback adapter
- See HRInstructions.txt for more Java instructions

----- HR Instructions -----

Dependencies
- Copy out Eclipse in C drive
- Install Java 8
- Set 192.168.XXX.XXX in iPV4, Advanced Settings --> This is for Luciad to run
- Install loop back adapter

Main code is in Main.Java
- Take note of CAMERA_PITCH_OFFSET --> The offset DEGREE of the camera
- Webcam Player will start --> It will break the code if the Webcam is not plugged in
- addOrUpdateTarget(id, lat, lon) --> Insert the Lat Lon of the enemy drone
- setCamera() --> to set the pose of the camera given position and roll pitch yaw
- UDPPORT for telemetry

TelemetryMessage.Java
- Contains all the functions for reading the telemetry

DetectorService.java (under vca)
- UDPPORT is the port that the program is listening for Ling Ling 
- drawDetection() --> Draws to the video
- DetectionMessage detection is the screen coordinates from Ling Ling

----- HR Instructions -----

Arduino:
- Install Arduino IDE 1.8.10
- Install Arduino Nano clone driver (CH341SER)
- Copy sketches over
- Copy mavlink library

Betaflight:
- Install Betaflight Configurator 10.6.0
- Copy configs over


Application flow:
GPS + Telemetry --> Arduino --> Xbee Transmitter --> Xbee Receiver --> USB Com Port --> Java app


FFMpeg screen capture commands (for external recording only):
ffmpeg -framerate 60 -pattern_type glob -i *.png -c:v libx264 -r 30 pix_fmt yuv420p out.mp4

ffmpeg -framerate 60 -i "Comp2_%.png" out.mp4

ffmpeg -framerate 60 -i "D:/Project Files/CounterSwarmDIF/Files/Slides/Screenshots/Dvd_Raw_CapFinal[1]/Dvd_Raw_CapFinal[1]_%5d.jpg" out.mp4




Large Files directories:
- Download the following large files from https://drive.google.com/drive/folders/13XjkhbjHvpFLrzAp_mK9lkg2Ipfy1iwY, and place them in the specified directories below before opening the project

DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\.svn\pristine\19\19bc653442bcde181ecb10fa53b65ab57363711f.svn-base
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\.svn\pristine\5f\5f34d1629c9bde53ecfa9807e80dd5ba04d80e80.svn-base
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\.svn\pristine\d8\d897ba01dd66e58e26ce61596956503e3af3f9fe.svn-base
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\.svn\pristine\fc\fc2b5980146e538d07e08b26086154327b19bec8.svn-base
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\resource\label_map2\model.ckpt-6506.data-00000-of-00001
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\resource\label_map2\model.ckpt-0.data-00000-of-00001
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\resource\label_map2\model.ckpt-2596.data-00000-of-00001
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\resource\ssd_inception_v2_coco_11_06_2017\model.ckpt.data-00000-of-00001
DvDFiles\DvDJava\dvdWorkspace\dvd.dif.video\resource\ssd_inception_v2_coco_11_06_2017\frozen_inference_graph.pb
DvDFiles\DvDJava\dvdWorkspace\luciad\.svn\pristine\e5\e51875be35fa23a88c6a521e30e36ece2cad6451.svn-base.zip
DvDFiles\DvDJava\dvdWorkspace\luciad\lib\lcd_geoid_resources.jar
DvDFiles\DvDJava\dvdWorkspace\vca\.svn\pristine\95\9514b4c1531a084ac40c7482b82bdf6909ceb6a2.svn-base
DvDFiles\DvDJava\dvdWorkspace\vca\.svn\pristine\d8\d897ba01dd66e58e26ce61596956503e3af3f9fe.svn-base
DvDFiles\DvDJava\dvdWorkspace\vca\lib\tensorflow_jni.dll
DvDFiles\DvDJava\dvdWorkspace\vca\resource\ssd_inception_v2_coco_11_06_2017\frozen_inference_graph.pb
DvDFiles\DvDJava\dvdWorkspace\vca\resource\ssd_inception_v2_coco_11_06_2017\model.ckpt.data-00000-of-00001
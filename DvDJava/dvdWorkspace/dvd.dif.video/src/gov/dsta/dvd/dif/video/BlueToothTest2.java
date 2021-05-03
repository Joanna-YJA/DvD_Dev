package gov.dsta.dvd.dif.video;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.bluetooth.DeviceClass;
import javax.bluetooth.DiscoveryAgent;
import javax.bluetooth.DiscoveryListener;
import javax.bluetooth.LocalDevice;
import javax.bluetooth.RemoteDevice;
import javax.bluetooth.ServiceRecord;
import javax.bluetooth.UUID;
import javax.microedition.io.Connector;
import javax.microedition.io.StreamConnection;

public class BlueToothTest2 {

    boolean scanFinished = false;
    RemoteDevice hc05device;
    String hc05Url;

    public static void main(String[] args) {
        try {
            new BlueToothTest2().go();

        } catch (Exception ex) {
            Logger.getLogger(BlueToothTest2.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    private void go() throws Exception {
     

       
    }
}
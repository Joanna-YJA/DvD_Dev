package gov.dsta.dvd.dif.video.player;

import java.util.ArrayList;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import org.bytedeco.ffmpeg.global.avutil;
import org.bytedeco.javacv.Frame;
import org.bytedeco.javacv.Java2DFrameConverter;
import org.bytedeco.javacv.OpenCVFrameGrabber;

import gov.dsta.dvd.dif.video.Main;
import gov.dsta.vca.Detection;
import gov.dsta.vca.DetectorService;
import javafx.application.Platform;
import javafx.embed.swing.SwingFXUtils;
import javafx.scene.image.ImageView;

public class WebcamPlayer {
	private final static int FRAME_COUNT_BETWEEN_DETECTION = 1;
	private ArrayList<Detection> detections;
	private int frameCounter = 0;
    private static volatile Thread playThread;
	private static ExecutorService detectionThread = Executors.newSingleThreadExecutor();
	
	private ImageView imageView = null;
	private int deviceIndex = 0; 
	public WebcamPlayer(ImageView imageView, int deviceIndex) {
		this.deviceIndex = deviceIndex;
		this.imageView = imageView;
	}

	public void start(){
		playThread = new Thread(new Runnable() { public void run() {
            try {
                avutil.av_log_set_level(avutil.AV_LOG_DEBUG);

            	OpenCVFrameGrabber grabber = new OpenCVFrameGrabber(deviceIndex);
            	grabber.start();
                final Java2DFrameConverter converter = new Java2DFrameConverter();
                ExecutorService executor = Executors.newSingleThreadExecutor();
                DetectorService vca = new DetectorService();
                vca.activate();
                while (!Thread.interrupted()) {
                	Frame frame = grabber.grab();
                    if (frame == null) {
                        break;
                    }

                    if (frame.image != null) {
                      //  Platform.runLater(new Runnable() { public void run() {
                        	Frame toProcess = frame;
                        	
                        	//LOCAL VA START
//                        	if(frameCounter > FRAME_COUNT_BETWEEN_DETECTION){
//       							detectionThread.execute(()->{
//       								//TODO change to jetson VA
//       							});
//       						    frameCounter = 0;
//       						}
//       						frameCounter++;
       						//TODO change to jetson VA
					
                        	//LOCAL VA STOP
                        	
                        	//EXTERNAL VA
     						//Frame frameToShow = vca.drawDetection(toProcess);
     						
     						if(Main.doVA){
     							detectionThread.execute(()->{
         						    detections = vca.detectWithNoDraw(vca.censorLabels(toProcess));
         						  double fps =  Main.fpsCounter.newEvent();
             						Frame frameToShow = vca.drawDetections(detections, toProcess, (int) fps);
//    								//TODO change to jetson VA
                                    imageView.setImage(SwingFXUtils.toFXImage(converter.convert(frameToShow), null));

    							});
     						}else{
//     							System.err.println(toProcess.imageWidth + " "+ toProcess.imageHeight);
                                imageView.setImage(SwingFXUtils.toFXImage(converter.convert(toProcess), null));

//                                imageView.setImage(SwingFXUtils.toFXImage(converter.convert(vca.censorLabels(toProcess)), null));

     						}
     						
                      //  }});
                    	 

                    } else if (frame.samples != null) {
//                       
                    }
                }
                executor.shutdownNow();
                executor.awaitTermination(10, TimeUnit.SECONDS);
                grabber.stop();
                grabber.release();
                Platform.exit();
            } catch (Exception e) {
            	e.printStackTrace();
                System.exit(1);
            }
        }});
        playThread.start();
	}
	
	public void stop() throws Exception {
        playThread.interrupt();
    }
}

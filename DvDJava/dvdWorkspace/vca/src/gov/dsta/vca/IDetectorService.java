package gov.dsta.vca;

import java.nio.ByteBuffer;
import java.util.ArrayList;

import javafx.scene.image.Image;

public interface IDetectorService {
	
	// perform detection
	public ArrayList<Detection> detect(ByteBuffer image, int rows, int cols, int channels, float threshold);
	
	// release resource, called only when the service is shut down
	public void close();

	Image drawDetections(ArrayList<Detection> detections, Image frame);

	ArrayList<Detection> detectWithNoDraw(Image frame);
}

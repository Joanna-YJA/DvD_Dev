package gov.dsta.vca;

import java.awt.Rectangle;
import java.nio.ByteBuffer;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

import org.bytedeco.javacv.Frame;
import org.bytedeco.javacv.OpenCVFrameConverter;
import org.opencv.core.Core;
import org.opencv.core.Mat;
import org.opencv.core.Point;
import org.opencv.core.Scalar;
//import org.opencv.core.Core;
//import org.opencv.core.Mat;
//import org.opencv.core.Point;
//import org.opencv.core.Scalar;
import org.opencv.imgproc.Imgproc;

import gov.dsta.vca.udp.DetectionMessage;
import gov.dsta.vca.udp.UdpServer;
import gov.dsta.vca.util.Utils;
import javafx.scene.image.Image;

public class DetectorService implements IDetectorService {

	private static final float DETECTION_THRESHOLD = 0.5f;
	private InceptionV2 inception = null;
	private static final String MSG_PREFIX = "@DetectorService:";
	private static final int _100 = 100;
	private static final float _0_35F = 0.35f;
	private static final double _255_0 = 255.0;
	private static final int _162 = 16;
	private OpenCVFrameConverter.ToOrgOpenCvCoreMat converterToOpenCvMat = new OpenCVFrameConverter.ToOrgOpenCvCoreMat();
	private UdpServer udpServer = null;
	private static Integer UDP_PORT = 4812;
	//TODO maybe need to extend to be a list because might detect more than one.
	private static DetectionMessage currentDetection = null;//new DetectionMessage("hello", 1, 300, 300, 700, 700);
	public void activate(){

		udpServer = new UdpServer(UDP_PORT).start();
		udpServer.addRequestListener((e->{
			currentDetection = (DetectionMessage) e.getCommand();
		}));
		System.loadLibrary(Core.NATIVE_LIBRARY_NAME);

		Thread t = new Thread(()->{
			// Inception V2 model and label map
			String modelDir = "resource/graph2";
			String labelDir = "resource/label_map2";
			String colorDir = "resource/label_map";

			// create an instance of the detector based on Inception V2
			inception = new InceptionV2();

			try {
				// initialize the inception by loading pre-trained model file and its label map

				inception.initialize(Paths.get(modelDir, "frozen_inference_graph.pb"),
						Paths.get(labelDir, "labelmap.pbtxt"), Paths.get(colorDir, "mscoco_color_map.txt"));


				/*
				// try
				inception.initialize(Paths.get(modelDir, "frozen_inference_graph.pb"),
						Paths.get(labelDir, "pet_label_map.pbtxt"), Paths.get(colorDir, "mscoco_color_map.txt"));

				 */

			}
			catch (Exception e) {
				//e.printStackTrace();
				e.printStackTrace();

			}
		});
		t.start();


	}

	public Frame drawDetection(Frame frame) {
		Mat input = converterToOpenCvMat.convert(frame);
		DetectionMessage detection = currentDetection;
		if (detection == null || (System.currentTimeMillis() - detection.getLastUpdateTime()) >500){
			return frame;
		}

		double scaleX = frame.imageWidth/640.0;//1920/frame.imageWidth;
		double scaleY = frame.imageHeight/480.0;//1080/frame.imageHeight;



		Scalar detectColor = new Scalar(detection.getBlue(), detection.getGreen(), detection.getRed(), _255_0);
		int minX = detection.getMinX();
		int minY = detection.getMinY();
		int width = detection.getWidth();
		int height = detection.getHeight();


		int scaledWidth = Double.valueOf(width * scaleX).intValue();
		int scaledHeight = Double.valueOf(height * scaleY).intValue();
		int scaledMinX = Double.valueOf(minX * scaleX).intValue();
		int scaledMinY = Double.valueOf(minY * scaleY).intValue();
		int scaledMaxX = scaledMinX + scaledWidth;
		int scaledMaxY = scaledMinY + scaledHeight;

		int x = scaledMinX;
		int y = scaledMinY - 10;
		if (y < 0)
			y = scaledMaxX + _162;
		String percentage = "%";
		String text = String.format("%s(%d%s)", detection.getName(), Float.valueOf(_100 * detection.getScore()).intValue(), percentage);
		Imgproc.putText(input, text, new Point(x, y), Core.IMPL_PLAIN, _0_35F, detectColor, 1);
		Imgproc.rectangle(input, new Point(scaledMinX, scaledMinY), new Point(scaledMaxX, scaledMaxY), detectColor,  2);

		return converterToOpenCvMat.convert(input);


	}
	List<Rectangle> filterList = new ArrayList<>();
	{
		Rectangle rect1 = new Rectangle(0,300,640,480-300);
		filterList.add(rect1);
	}

	private boolean checkIfWithinBound(Detection detection) {
		for(Rectangle rect :filterList){
			if(rect.contains(new java.awt.Point(detection.getMinX(),detection.getMinY()))){
				return true;
			}
		}
		return false;
	}
	public Frame censorLabels(Frame frame) {
		Mat input = converterToOpenCvMat.convert(frame);
		Scalar detectColor = new Scalar(0,0,0, _255_0);

		Imgproc.rectangle(input, new Point(0, 350), new Point(frame.imageWidth, frame.imageHeight), detectColor,Imgproc.FILLED,  0);

		return converterToOpenCvMat.convert(input);
	}

	public Frame drawDetections(ArrayList<Detection> detections, Frame frame, int fps) {
		Mat input = converterToOpenCvMat.convert(frame);

		Imgproc.putText(input, ""+fps, new Point(50, 50), Core.IMPL_PLAIN, _0_35F, new Scalar(0, 255, 0, _255_0), 1);

		if (detections == null || detections.size() == 0)
			return converterToOpenCvMat.convert(input);

		int count = detections.size();
		//System.err.println(count);
		for (int i = 0; i < count;i++) {
			Detection detection = detections.get(i);
			
			if(detection.getScore() < 0.95 ){
				continue;
			}
//			if(detection.getScore() < 0.95 || checkIfWithinBound(detection) ){
//				continue;
//			}
			Scalar detectColor = new Scalar(detection.getBlue(), detection.getGreen(), detection.getRed(), _255_0);
			int minX = detection.getMinX();
			int minY = detection.getMinY();
			int maxX = detection.getMaxX();
			int maxY = detection.getMaxY();
			int x = minX;
			int y = minY - 10;
			if (y < 0)
				y = maxY + _162;
			String percentage = "%";
			String text = String.format("%s(%d%s)", detection.getName(), Float.valueOf(_100 * detection.getScore()).intValue(), percentage);
			Imgproc.putText(input, text, new Point(x, y), Core.IMPL_PLAIN, _0_35F, detectColor, 1);
			Imgproc.rectangle(input, new Point(minX, minY), new Point(maxX, maxY), detectColor,  2);

			//return converterToOpenCvMat.convert(input);

		}
		return converterToOpenCvMat.convert(input);
	}

	public ArrayList<Detection> detectWithNoDraw(Frame frame) {

		Mat input = converterToOpenCvMat.convert(frame);

		int size = input.rows() * input.cols() * input.channels();

		if (size == 0)
			return null;

		byte[] bytes = new byte[size];
		input.get(0, 0, bytes);

		return detect(ByteBuffer.wrap(bytes), input.rows(), input.cols(),input.channels(), DETECTION_THRESHOLD);	}

	@Override
	public Image drawDetections(ArrayList<Detection> detections, Image frame) {
		Mat input = Utils.image2Mat(frame);

		if (detections == null || detections.size() == 0)
			return frame;

		int count = detections.size();
		for (int i = 0; i < count; i++) {
			Detection detection = detections.get(i);
			Scalar detectColor = new Scalar(detection.getBlue(), detection.getGreen(), detection.getRed(), _255_0);
			int minX = detection.getMinX();
			int minY = detection.getMinY();
			int maxX = detection.getMaxX();
			int maxY = detection.getMaxY();
			int x = minX;
			int y = minY - 10;
			if (y < 0)
				y = maxY + _162;
			String percentage = "%";
			String text = String.format("%s(%d%s)", detection.getName(), Float.valueOf(_100 * detection.getScore()).intValue(), percentage);
			Imgproc.putText(input, text, new Point(x, y), Core.IMPL_PLAIN, _0_35F, detectColor, 1);
			Imgproc.rectangle(input, new Point(minX, minY), new Point(maxX, maxY), detectColor,  2);

			Image imageToShow =Utils.mat2Image(input);

			return imageToShow;

		}
		return frame;
	}
	@Override
	public ArrayList<Detection> detectWithNoDraw(Image frame) {

		Mat input = Utils.image2Mat(frame);

		int size = input.rows() * input.cols() * input.channels();

		if (size == 0)
			return null;

		byte[] bytes = new byte[size];
		input.get(0, 0, bytes);
		ArrayList<Detection> result = detect(ByteBuffer.wrap(bytes), input.rows(), input.cols(),input.channels(), DETECTION_THRESHOLD);

		return result;
	}

	@Override
	public ArrayList<Detection> detect(ByteBuffer image, int rows, int cols, int channels, float threshold) {

		ArrayList<Detection> detections = null;

		try {
			//if (inception == null)
			//System.err.println("DetectorService not started properly!");

			// detecting
			detections = inception.detect(image, rows, cols, channels, DETECTION_THRESHOLD);
		}
		catch (Exception e) {
			e.printStackTrace();
		}

		return detections;
	}

	@Override
	public void close() {
		if (inception != null)
			inception.clean();
	}

}


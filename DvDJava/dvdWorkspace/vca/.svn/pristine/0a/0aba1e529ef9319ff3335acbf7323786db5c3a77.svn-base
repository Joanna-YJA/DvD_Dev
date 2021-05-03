/*
 * 
 * Java sample codes for demonstrating how to use Tensorflow for Java object detection API
 * 
 * @author Huang Tingxing
 * @version 1.0, September 9, 2017
 */


package gov.dsta.vca;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;

import org.opencv.core.Core;
import org.opencv.core.Mat;
import org.opencv.core.Point;
import org.opencv.core.Scalar;
import org.opencv.imgproc.Imgproc;
import org.tensorflow.DataType;
import org.tensorflow.Tensor;
import org.tensorflow.types.UInt8;

public class DataUtils {
	private static final int _255 = 255;
	private static final int _256 = 256;
	private static final int _128 = 128;
	private static final int _0X_FF = 0xFF;
	private static final double _255_0 = 255.0;
	private static final double _259_0 = 259.0;
	// look-up table
	private static int[] lut = new int[_256];
	private static double storedGamma = -1;		// 0.25: darker, 2.0: brighter

	
	private static int numScores = 0;
	private static float[][] detectedScores = null;
	private static int numClasses = 0;
	private static float[][] detectedClasses = null;
	private static int numBoxes = 0;
	private static float[][][] detectedBoxes = null;
	
	// template 0: MS COCO PBTXT, 1: some other format
	public static Map<Integer, IItem> parseLabelMap(Path labelMapPath, Path colorMapPath) throws IOException {
		
		LabelMapParser labelParser = new LabelMapParser();
		HashMap<Integer, IItem> labelItems = labelParser.load(labelMapPath.toString());
		labelParser.close();
		//System.out.println("Number of items loaded: " + labelItems.size());
		
		ColorMapParser colorParser = new ColorMapParser();
		HashMap<Integer, IItem> colorItems = colorParser.load(colorMapPath.toString());
		
		for (Map.Entry<Integer, IItem> entry : colorItems.entrySet()) {
			IItem item = labelItems.get(entry.getKey());
			if (item == null)
				continue;
			
			LabelItem labelItem = (LabelItem)item;
			
			ColorItem colorItem = (ColorItem) entry.getValue();
			labelItem.setRgb(colorItem.getRed(), colorItem.getGreen(), colorItem.getBlue());
		}
		
		colorItems = null;
		
		return labelItems;
	}
	
	// template 0: MS COCO PBTXT, 1: some other format
	public static Map<Integer, IItem> parsePetLabelMap(Path labelMapPath, Path colorMapPath) throws IOException {
		
		PetLabelParser labelParser = new PetLabelParser();
		HashMap<Integer, IItem> labelItems = labelParser.load(labelMapPath.toString());
		labelParser.close();
		//System.out.println("Number of items loaded: " + labelItems.size());
		
		ColorMapParser colorParser = new ColorMapParser();
		HashMap<Integer, IItem> colorItems = colorParser.load(colorMapPath.toString());
		
		for (Map.Entry<Integer, IItem> entry : colorItems.entrySet()) {
			IItem item = labelItems.get(entry.getKey());
			if (item == null)
				continue;
			
			LabelItem labelItem = (LabelItem)item;
			
			ColorItem colorItem = (ColorItem) entry.getValue();
			labelItem.setRgb(colorItem.getRed(), colorItem.getGreen(), colorItem.getBlue());
		}
		
		colorItems = null;
		
		return labelItems;
	}

	
	public static Tensor getImageTensor(ByteBuffer byteBuffer, int rows, int cols, int channels) {
		
		if (byteBuffer == null)
			return null;
		
		// construct Shape
        long[] shape = new long[4];
        shape[0] = 1;
        shape[1] = rows;
        shape[2] = cols;
        shape[3] = channels;
    
        return Tensor.create(UInt8.class, shape, byteBuffer);
	}
	
	public static boolean isTensorValid(Tensor tensor, int expectedDimensions) throws RuntimeException {
		
		boolean valid = true;
		
		  
	  	final long[] shape = tensor.shape();
	    if (tensor.numDimensions() != expectedDimensions || shape[0] != 1) {
	    	String rightStruct = (expectedDimensions == 3? "[1 N 4]" : "[1 N]");
	    	valid = false;
	        throw new RuntimeException(
	            String.format("Expected model to produce a %s shaped tensor where N is the number of labels, instead it produced one with shape %s",
	            		rightStruct, Arrays.toString(shape)));
	    }
	    
	    return valid;
	}
	
	public static ArrayList<Integer> findIndicesAboveThreshold(float threshold, float[] scores) {
		ArrayList<Integer> items = new ArrayList<Integer>();
		
		int length = scores.length;
		for (int i = 0; i < length; i++) {
			if (scores[i] < threshold)
				continue;
			
			items.add(i);
		}
		
		return items;
	}
	
	public static float[] getScores(Tensor tensorScores) throws Exception {
		if (tensorScores == null || !isTensorValid(tensorScores, 2))
			return null;
		
		
		final long[] scoresShape = tensorScores.shape();
		Long scores = scoresShape[1];
		int numScoresNow = scores.intValue();
		if (detectedScores == null || numScoresNow != numScores) {
			numScores = numScoresNow;
			detectedScores = new float[1][numScores];
		}
    	tensorScores.copyTo(detectedScores);
    	
    	return detectedScores[0];
	}

	public static float[] getClasses(Tensor tensorClasses) {
		if (tensorClasses == null || !isTensorValid(tensorClasses, 2))
			return null;
		
		final long[] classesShape = tensorClasses.shape();
		int numClassesNow = Long.valueOf(classesShape[1]).intValue();
		if (detectedClasses == null || numClassesNow != numClasses) {
			numClasses = numClassesNow;
			detectedClasses = new float[1][numClasses];
		}
		tensorClasses.copyTo(detectedClasses);
    	
    	return detectedClasses[0];
	}
	
	public static float[][] getBoxes(Tensor tensorBoxes) {
		if (tensorBoxes == null || !isTensorValid(tensorBoxes, 3))
			return null;
		
		final long[] boxesShape = tensorBoxes.shape();
		int numBoxesNow = Long.valueOf(boxesShape[1]).intValue();
		if (detectedBoxes == null || numBoxesNow != numBoxes) {
			numBoxes = numBoxesNow;
			detectedBoxes = new float[1][numBoxes][4];
		}
    	tensorBoxes.copyTo(detectedBoxes);
    	
    	return detectedBoxes[0];
	}
	
	public static void drawBox(Mat image, int minX, int minY, int maxX, int maxY, int thick, Scalar color, String text) {
		
		int posX = minX;
		int posY = minY > 4? minY - 4 : 0;
		
		Imgproc.putText(image, "Person", new Point(posX, posY), Core.IMPL_PLAIN, 1.0, new Scalar(0, 0, _255, _255));
    	
		Imgproc.rectangle(image, new Point(minX, minY), new Point(maxX, maxY), color,  thick);
	}
	
	// gamma 0.25 to 2.2
	private static void initGammaLUTable(double gamma) {
		
		storedGamma = gamma;
		
		double invGamma = 1.0/gamma;
	
		for (int i = 0; i < _256; i++) {
			Double gammaDouble = Math.pow(i/_255_0, invGamma);
			lut[i] = gammaDouble.intValue() * _255;
		}
		
	}
	
	private static int truncate(int value) {
		if (value < 0)
			return 0;
		
		if (value > _255)
			return _255;
		
		return value;
	}
	
	// gamma: 0.25 to 2.2
	public static void adjustGamma(byte[] bytes, int size, double gamma) {
//		if (storedGamma != gamma)
		if(Double.compare(storedGamma, gamma) != 0)
			initGammaLUTable (gamma);
		
		for (int i = 0; i < size; i++) {
			bytes[i] =  Integer.valueOf(lut[(int)bytes[i] & _0X_FF]).byteValue();
		}
	}
	
	// brightness -255 to 255
	public static void adjustBrightness(byte[] bytes, int size, int brightness) {
		for (int i = 0; i < size; i++) {
			bytes[i] =  Integer.valueOf(truncate((int)bytes[i] & _0X_FF + brightness)).byteValue();
		}
	}
	
	// contrast -255 to 255
	public static void adjustContrast(byte[] bytes, int size, int contrast) {
		double factor = (_259_0 * (contrast + _255_0)) / (_255_0 * (_259_0 - contrast));
		for (int i = 0; i < size; i++) {
			bytes[i] = Integer.valueOf(truncate(Double.valueOf(factor * ((int)bytes[i] & _0X_FF - _128) + _128).intValue())).byteValue();
		}
	}

}

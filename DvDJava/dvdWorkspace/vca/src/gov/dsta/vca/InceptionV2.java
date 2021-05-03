/*
 * 
 * Java sample codes for demonstrating how to use Tensorflow for Java object detection API
 * 
 * @author Huang Tingxing
 * @version 1.0, September 9, 2017
 */


package gov.dsta.vca;

import java.awt.Color;
import java.nio.ByteBuffer;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Map;
import java.util.logging.Logger;

import org.tensorflow.Graph;
import org.tensorflow.Session;
import org.tensorflow.Tensor;


public class InceptionV2 {
	private static final int _85 = 85;
	private static final int _232 = 232;
	private static final String MSG_PREFIX = "@InceptionV2:";
	private Graph graph = null;
	private Session session = null;
	private Map<Integer, IItem> labelMap = null;
	
	private boolean loaded = false;
	
	public InceptionV2() {
		loaded = false;
	}
	
	public void initialize(Path modelPath, Path labelMapPath, Path colorMapPath) throws Exception {
		
		if (loaded)
			return;
		
		// load MSCOCO label map, and color map
	    labelMap = DataUtils.parseLabelMap(labelMapPath, colorMapPath);
	    
		// htx test
	    //labelMap = DataUtils.parsePetLabelMap(labelMapPath, colorMapPath);

	    // load graph definition
	    byte[] graphDef = Files.readAllBytes(modelPath);
	    
	    // import graph definition into Graph object
	    graph = new Graph();
	    graph.importGraphDef(graphDef);
	    session = new Session(graph);
	    
	    loaded = true;
	}
	
	public String getLabel(int detectionId) {
		if (!loaded)
			return "";
		
		LabelItem item = (LabelItem) labelMap.get(detectionId);
		if (item == null)
			return "";
		
		return item.getLabel();
	}
	
	public Color getColor(int detectionId) {
		if (!loaded)
			return null;
		
		LabelItem item = (LabelItem) labelMap.get(detectionId);
		if (item == null)
			return null;
		
		return new Color(item.getRed(), item.getGreen(), item.getBlue());
	}

	public Tensor[] performDetection(Tensor image) throws RuntimeException {
		if (!loaded)
			throw new RuntimeException("Detector model not loaded");
		Tensor[] tensors = new Tensor[3];

    	session.runner().feed("image_tensor", image).fetch("detection_classes").fetch("detection_scores").fetch("detection_boxes").run().toArray(tensors);
		
    	return tensors;
	}
	
	public ArrayList<Detection> detect(ByteBuffer image, int rows, int cols, int channels,  float threshold) {
		if (image == null)
			return null;
		
		Tensor imageTensor = DataUtils.getImageTensor(image, rows, cols, channels);
		if (imageTensor == null)
			return null;
		
		ArrayList<Detection> detections = new ArrayList<>();
		
		try {
//			long timeBefore = System.currentTimeMillis();
			Tensor[] tensors = performDetection(imageTensor);
//			System.err.println(System.currentTimeMillis() - timeBefore);
			if (tensors == null || tensors.length != 3)
				return null;
			
			float[] classesValue = DataUtils.getClasses(tensors[0]);
			float[] scoresValue = DataUtils.getScores(tensors[1]);
			float[][] boxesValue = DataUtils.getBoxes(tensors[2]);
			
			ArrayList<Integer> detected = DataUtils.findIndicesAboveThreshold(threshold, scoresValue);
			if (detected == null)
				return null;
			
        	int detectionCount = detected.size();
        	for (int i = 0; i < detectionCount; i++) {
        		int index = detected.get(i).intValue();
        		
        		// detected label, and color
        		int classID = Float.valueOf(classesValue[index]).intValue();
        		String label = "";
        		int red = _232;
        		int green = _232;
        		int blue = _85;
        		if (labelMap != null) {
        			LabelItem labelItem = (LabelItem)labelMap.get(classID);
        			if (labelItem != null) {
        				label = labelItem.getLabel();
        				red = labelItem.getRed();
        				green = labelItem.getGreen();
        				blue = labelItem.getBlue();
        			}
        		}
        		
        		// detected score
        		float score = scoresValue[index];
        		
        		// detected box
        		float[] box = boxesValue[index];
        		int minY =  Float.valueOf((box[0] * rows)).intValue();
        		int minX =  Float.valueOf((box[1] * cols)).intValue();
        		int maxY =  Float.valueOf((box[2] * rows)).intValue();
        		int maxX =  Float.valueOf((box[3] * cols)).intValue();
        		
        		detections.add(new Detection(label, score, minX, minY, maxX, maxY, red, green, blue));
        	}
        	
        	return detections;
		}
		catch (Exception e) {

		}
		
		return null;
	}
	
	public void clean() {
		if (!loaded)
			return;
		
		if (session != null)
			session.close();
		
		if (graph != null)
			graph.close();
	}
}

package gov.dsta.luciad;

import java.util.Hashtable;

import gov.dsta.luciad.interfaces.IKLayer;

public final class LayerController {
	
	private static Hashtable<String, IKLayer> layersTable = new Hashtable<String, IKLayer>();
	private static Hashtable<String, IKLayer> mapLayersTable = new Hashtable<String, IKLayer>();
	
	public static void setLayersTable(Hashtable<String, IKLayer> newLayersTable){
		if(null != newLayersTable){
			layersTable = newLayersTable;
		}
	}
	
	public static Hashtable<String, IKLayer> getLayersTable(){
		return layersTable;
	}
	
	public static boolean isLayerExist(String layerName){
		return layersTable.containsKey(layerName);
	}
	
	public static void addMapLayer(String layerName, IKLayer layer){
		layersTable.put(layerName, layer);
		mapLayersTable.put(layerName, layer);
	}
	
	public static Hashtable<String, IKLayer> getMapLayersTable(){
		return mapLayersTable;
	}
}
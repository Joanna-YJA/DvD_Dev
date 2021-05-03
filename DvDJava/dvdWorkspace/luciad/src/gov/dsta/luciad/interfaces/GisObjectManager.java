package gov.dsta.luciad.interfaces;


import java.util.HashMap;
import java.util.Map;



/**
 * This class manage two way relationship between application graphic model and GIS object.
 * 
 * @author KWM 
 * @version $Id: GisObjectManager.java,v 1.2 2015/05/22 08:17:51 fhr Exp $
 * 
 */
public class GisObjectManager {


	private static Map<Object, IKGraphic> gisObjectsToModelTableNew = new HashMap<Object, IKGraphic>();
	private static Map<IKGraphic, Object> modelToGisObj = new HashMap<IKGraphic, Object>();

	public static void addObject(Object gisObject, IKGraphic model){
		gisObjectsToModelTableNew.put(gisObject, model);
		modelToGisObj.put(model, gisObject);
	}
	
	public static void removeObject(Object gisObject, IKGraphic model){
		gisObjectsToModelTableNew.remove(gisObject);
		modelToGisObj.remove(model);

		
	}
	
	public static IKGraphic getModel(Object gisObject){
		if(null == gisObject)
			return null;
		else
			return gisObjectsToModelTableNew.get(gisObject);
	}
	
	public static Object getGISObj(IKGraphic model) {
		return modelToGisObj.get(model);
	}
	
}

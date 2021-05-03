package gov.dsta.luciad.interfaces;

import java.util.List;


public interface IKLayer {
	public String getName();
	public void setName(String name);
	public int getCount();
	public void addGraphic(IKGraphic graphic);
	public IKGraphic getGraphicAt(int index);
	public void removeGraphic(IKGraphic graphic);
	public boolean isExist(IKGraphic graphic);
	public void deleteAllModels();
	public void setReducedDisplay(boolean reducedDisplay);
	public boolean isReducedDisplaySupported();
	public List<IKGraphic> getAllGraphics();
	public void setReferenceObject(Object ref);
	public Object getReferenceObject();
	public LAYER_TYPE getLayerType();
	public static enum LAYER_TYPE {BACKGROUND, INTERACTIVE, EDITABLE, REALTIME}
	void setLayerType(LAYER_TYPE layerType);;

}

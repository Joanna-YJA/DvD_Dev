package gov.dsta.luciad.interfaces;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Vector;

public class KLayer implements IKLayer {
	private String name;
//	private int index;
	private Object referenceObject; 
	private final List<IKGraphic> graphicList = new ArrayList<IKGraphic>();
	private boolean reducedDisplay = false;
	private LAYER_TYPE layerType = LAYER_TYPE.EDITABLE;
	public KLayer(String name, int index){
		this.name = name;
		//this.index = index;
	}
	
	public KLayer(Vector<IKGraphic> graphicList2) {
		for (IKGraphic m : graphicList2) {
			if(!isExist(m))
				addGraphic(m);
		}
	}

	public String getName(){
		return this.name;
	}
	
	public void setName(String name){
		this.name = name;
	}
	
	@Override
	public int getCount() {
		return graphicList.size();
	}

	@Override
	public void addGraphic(IKGraphic graphic) {
		graphicList.add(graphic);
		
	}
	
	@Override
	public void removeGraphic(IKGraphic graphic) {
		graphicList.remove(graphic);
		
	}

	@Override
	public IKGraphic getGraphicAt(int index) {
		return graphicList.get(index);
	}
	@Override
	public boolean isExist(IKGraphic graphic){
		return graphicList.contains(graphic);
	}
	@Override
	public List<IKGraphic> getAllGraphics(){

		//create a clone list
		List<IKGraphic> cloneList = new ArrayList<IKGraphic>();
		Iterator<IKGraphic> iterator = graphicList.iterator();
		
		//add in the models from modelList to cloneList
		while(iterator.hasNext()){
			cloneList.add(iterator.next());
		}
		
		return cloneList;
	}
	@Override
	public void deleteAllModels(){
		List<IKGraphic> modelList = getAllGraphics();
		
		for (IKGraphic m : modelList) {
			removeGraphic(m);
		}
	}
	@Override
	public void setReducedDisplay(boolean reducedDisplay) {
		this.reducedDisplay = reducedDisplay;
	}
	@Override
	public boolean isReducedDisplaySupported() {
		return reducedDisplay;
	}
	@Override
	public void setReferenceObject(Object ref){
		this.referenceObject = ref;
	}
	@Override
	public Object getReferenceObject(){
		return this.referenceObject;
	}


	
	private Vector<IKGraphic> getGraphicVectorList(){
		Vector<IKGraphic> graphicVectorList = new Vector<IKGraphic>();
		for (IKGraphic m : graphicList) {
			graphicVectorList.add(m);
		}
		return graphicVectorList;
	}

	@Override
	public LAYER_TYPE getLayerType() {
		return layerType;	
	}
	@Override
	public void setLayerType(LAYER_TYPE layerType) {
		this.layerType = layerType;
	}
	

}

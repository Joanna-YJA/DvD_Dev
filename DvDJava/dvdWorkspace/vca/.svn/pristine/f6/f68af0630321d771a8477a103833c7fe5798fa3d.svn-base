package gov.dsta.vca;

import java.awt.Color;
import java.io.IOException;

public class ColorMapParser extends ItemParser {
	
	private Color defaultColor = new Color(255, 255, 255);
	
	/*public ColorMapParser() {
		
	}*/

	@Override
	public IItem parseItem() throws IOException {
		String s = readUnquotedString();
    	if (s == null || s.isEmpty())
    		return null;
    	
        if (("item").equals(s.toLowerCase())) {
        	return readItem();
        }

        return null;
	}
	
	// item: item {
    //			id: 1: 1
    //	  		display_name: "person"
    //			display_color: "green"
    //		}
    protected IItem readItem() throws IOException {
    	
		if (readNext() != CURLY_BRACE_OPEN)
			throw new IOException(getMessage(MISSING_CURLY_BRACE + " {", _lineNumber));
		
		IItem item = readItemBody();
		
		if (readNext() != CURLY_BRACE_CLOSE)
			throw new IOException(getMessage(MISSING_CURLY_BRACE + " }", _lineNumber));
			
		return item;
	}

    private IItem readItemBody() throws IOException {
       // String labelValue = "";
        int idValue = -1;
        String colorString = "";
        
    	do {
            String key = readAttribute();
            if (key == null || key.isEmpty())
            	break;
            
            if ("id".equals(key.toLowerCase()))
            	idValue = readInt();
            else if ("display_name".equals(key.toLowerCase())){
            	readString();
            }
            else if ("display_color".equals(key.toLowerCase()))
            	colorString = readString();
            
            skipWhiteSpace();
            
        } while (peek() != CURLY_BRACE_CLOSE);
    	
    	if (idValue < 0)
    		return null;
    	
    	Color color = ItemParser.toColor(colorString, defaultColor);
    	
    	return new ColorItem(idValue, color.getRed(), color.getGreen(), color.getBlue());
    }
}

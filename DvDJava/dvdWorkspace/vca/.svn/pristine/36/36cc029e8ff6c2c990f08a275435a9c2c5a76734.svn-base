package gov.dsta.vca;

import java.io.IOException;

public class PetLabelParser extends ItemParser {
	

	
	@Override
	public IItem parseItem() throws IOException {
		String s = readUnquotedString();
    	if (s == null || s.isEmpty())
    		return null;
    	
        if ("item".equals(s.toLowerCase())) {
        	return readItem();
        }

        return null;

	}
	
	// item: item {
	//			id: 1
    //			name: "/m/01g317"
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
    	
        String nameValue = "";
        int idValue = -1;
    //    String labelValue = "";
        
    	do {
            String key = readAttribute();
            if (key == null || key.isEmpty())
            	break;
            
            if ("id".equals(key.toLowerCase()))
            	idValue = readInt();
            else if ("name".equals(key.toLowerCase()))
            	nameValue = readString();
            //else if ("display_name".equals(key.toLowerCase()))
            //	labelValue = readString();
            
            skipWhiteSpace();
            
        } while (peek() != CURLY_BRACE_CLOSE);
    	
    	if (idValue < 0)
    		return null;
    	
        return new LabelItem(nameValue, idValue, nameValue);
    }
}

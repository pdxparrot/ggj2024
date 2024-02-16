extends Label

# Expose the stringList variable in the editor
var stringList = ["Shane Lillie", "Tyler Richardson", "Tien Tam"]

func _ready():
    # Call the function to replace the variable placeholder
    replaceVariable()

func replaceVariable():
    # Define your text string with the variable placeholder
    var textString = "A %variable% Game"
    
    # Replace the variable placeholder with a random string from the list
    var randomString = stringList[randi() % stringList.size()]
    textString = textString.replace("%variable%", randomString)
    
    # Set the Label text to the modified string
    text = textString

# Gist Integration Addin

> In order to use this sample page make sure to install the [Markdown Monster Gist Addin](https://github.com/RickStrahl/GistIntegration-MarkdownMonster-Addin) and ensure that the **Allow Script Tags in Markdown** is enabled
> 


You can embed code as Gists into a Markdown Document in couple of ways:

* Select an existing Gist and embed it
* Create a new Gist, publish it, and embed it

### Embedding an Existing Gist
To embed an existing Gist:

* Use the **Embed or Open Gist** from the Gist Toolbar Icon dropdown
* or: Use **File->Open From-> Open or Embed from Gist** Menu
* Select a Gist in the list
* **Double Click** or use **Embed Gist** to embed

### Creating a new Gist
There are a few ways to create a new Gist:

* Select some code in the editor and open **Gist Integration**
* or: **Open Gist Integration and manually edit code
* Edit your code in the editor
* Click on **Publish and Embed**

<script src="https://gist.github.com/39b952dd0605984c960b690720094fcc.js"></script>

This features takes the **current Clipboard or Editor text selection** and publishes it as a Gist on Github, then 

A separate editor screen pops up that lets you optionally format the code before posting it to a Gist. You can post anonymous or account based Gists
, and use public or private visibility.

Here's some example code. Let's publish it:

```csharp
public static JObject CreateGistPostJson(GistItem gist)
{
    dynamic obj = new JObject();

    obj.Add("description", new JValue(gist.description));
    obj.Add("public", gist.isPublic);
    obj.Add("files", new JObject());

    obj.files.Add(gist.filename, new JObject());

    var fileObj = obj.files[gist.filename];
    fileObj.content = gist.code;

    return obj;
}
```


It'll detect syntax from existing fenced code blocks:

```foxpro
LOCAL lcText
lcText = "Hello"
lcText = lcText + " World"
```




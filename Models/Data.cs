using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

using System.Xml.Linq;
using System.Linq;

namespace Blog.Models;

public class Data
{
    private List<Publication> Publications { get; set; }
    private IEnumerable<Publication> publication;
    
    private XElement? root;

    public IEnumerable<Publication> Publication
    {
        get
        {
            return Publications.OrderByDescending( (e) => e.Fecha);
        } 

        private set
        {
            publication = value;
        }
    }

    string ruta, name;
    public Data(string ruta, string name)
    {
        this.ruta = ruta;
        this.name = name;
        Cargar();
    }

    public void Cargar()
    {
        if (String.IsNullOrEmpty(ruta))
            return;

        using var v = File.Open(ruta + "/" + name, FileMode.Open);

        root = XElement.Load(v);

        Publications = new();

        Publications.AddRange(root.Elements()
                    .SelectMany(EntryBlog => EntryBlog.Elements()
                    .Select(entry => new Publication()
                    {
                        Fecha = DateOnly.Parse(entry.Attribute("fecha").Value),
                        Title = entry.Attribute("title").Value,
                        Content = entry.Value
                    })));

        Publication = Publications;
    }
    public void Actualizar(Publication publicacion, Publication vieja)
    {
        if (root == null) return;

        var index = FindPublicationIndex(vieja.Title);

        var element = root.FindElement((xe) => xe.Attribute("fecha").Value == vieja.Fecha.ToString())
                          .FindElement((xe) => xe.Attribute("title").Value == vieja.Title);

        if (vieja.Fecha == publicacion.Fecha)
        {
            element.Value = publicacion.Content;
            element.Attribute("title").Value = publicacion.Title;
            element.Attribute("fecha").Value = publicacion.Fecha.ToString();
            GuardarToArchive();
        }
        else
        {
            EliminarXElement(element);
            Add(publicacion);
        }


        Publications[index] = publicacion;
    }

    private void GuardarToArchive()
    {
        using var stream = File.Open(ruta + "/" + name, FileMode.Truncate);
        root.Save(stream);
    }
    private int FindPublicationIndex(string nombre) =>
        Publications.FindIndex((e) => e.Title == nombre);

    public void Remove(string nombre)
    {
        if (root == null) return;

        var index = FindPublicationIndex(nombre);
        var element = Publications[index];

        var el = root.FindElement((xe) => xe.Attribute("fecha").Value == element.Fecha.ToString())
                     .FindElement((xe) => xe.Attribute("title").Value == element.Title);

        EliminarXElement(el);
        GuardarToArchive();

        Publications.RemoveAt(index);
    }

    private void EliminarXElement(XElement element)
    {
        XElement parent = element.Parent!;
        element.Remove();

        if (parent.IsEmpty)
            parent.Remove();
    }

    public void Add(Publication publicacion)
    {
        if (root == null) return;

        var blogEntry = root.FindElement((element) => element.Attribute("fecha").Value == publicacion.Fecha.ToString());

        if (blogEntry == root)
            root.Add(new XElement("BlogEntry",
                        new XAttribute("fecha", publicacion.Fecha.ToString()),
                        ToEntry(publicacion)));
        else
            blogEntry.Add(ToEntry(publicacion));


        GuardarToArchive();
        Publications.Add(publicacion);
    }

    private XElement ToEntry(Publication publicacion) =>
        new XElement("Entry", new XAttribute("title", publicacion.Title),
                              new XAttribute("fecha", publicacion.Fecha.ToString()),
                              publicacion.Content);
}

static class XElementExtension
{

    public static XElement FindElement(this XElement rot, Func<XElement, bool> condicion) => rot.Elements()
            .FirstOrDefault(condicion, rot);

    /* public static XElement FindBlogEntryElement(this XElement rot, string fecha) => rot.Elements()
             .First((blogEntry) => blogEntry.Attribute("fecha").Value == fecha);

     public static XElement FindEntryElement(this XElement root, string title) => root.Elements()
         .First((entry) => entry.Attribute("title").Value == title);*/
}
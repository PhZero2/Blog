using System;
using System.ComponentModel.DataAnnotations;
namespace Blog.Models;

public class Publication
{
    [Required(ErrorMessage="Seleccione una fecha correcta")]
    [DataType(DataType.Date)]
    
    public DateOnly Fecha {get; set;} 

    [Required(ErrorMessage="Escriba un titulo")]
    public string Title {get; set;} = String.Empty;
    public string Content {get; set;} = String.Empty;
    
    public Publication(){}
    
}
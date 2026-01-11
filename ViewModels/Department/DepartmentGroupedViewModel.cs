
using FRAProject.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Areas.HR.Models;
public class DepartmentGroupedViewModel
{
    public string BaseName { get; set; }
    public List<Department> Departments { get; set; }
}
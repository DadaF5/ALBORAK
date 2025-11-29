using FRAProject.DTOs;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.Base
{
    public class BaseViewModel
    {
        public BaseCreateDto Base { get; set; } = new BaseCreateDto();

    }
}

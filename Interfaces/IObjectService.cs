using StajP.DTOs;
using System.Collections.Generic;

namespace StajP.Interfaces
{
    public interface IObjectService
    {
        object Add(ObjectDto dto);
        object AddRange(List<ObjectDto> dtos);
        object GetAll();
        object GetById(int id);
        object Update(int id, ObjectDto dto);
        object Delete(int id);
    }
}
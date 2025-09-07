using StajP.DTOs;
using StajP.Interfaces;
using StajP.Properties;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using ObjectEntity = StajP.Entities.Object;

namespace StajP.Services
{
    public class StaticListObjectService : IObjectService
    {
        private static List<ObjectEntity> _objects = new List<ObjectEntity>();
        private static int _idCounter = 1;
        private readonly WKTReader _wktReader = new WKTReader();

        private List<string> ValidateDto(ObjectDto dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);
            return results.Select(r => r.ErrorMessage).ToList();
        }

        private ObjectEntity ConvertToEntity(ObjectDto dto)
        {
            try
            {
                return new ObjectEntity
                {
                    Id = _idCounter++,
                    Name = dto.Name,
                    Geometry = _wktReader.Read(dto.Wkt)
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException("WKT formatı geçersiz.", ex);
            }
        }

        public object Add(ObjectDto dto)
        {
            var errors = ValidateDto(dto);
            if (errors.Any())
            {
                return Response.Fail(messages.InvalidData + ": " + string.Join("; ", errors));
            }
            try
            {
                var entity = ConvertToEntity(dto);
                _objects.Add(entity);
                return Response.Success(entity, messages.PointCreated);
            }
            catch (ArgumentException ex)
            {
                return Response.Fail(ex.Message);
            }
        }

        public object AddRange(List<ObjectDto> dtos)
        {
            var allErrors = new List<string>();
            foreach (var dto in dtos)
                allErrors.AddRange(ValidateDto(dto));

            if (allErrors.Any())
            {
                return Response.Fail(messages.InvalidData + ": " + string.Join("; ", allErrors));
            }

            var resultEntities = new List<ObjectEntity>();
            foreach (var dto in dtos)
            {
                try
                {
                    var entity = ConvertToEntity(dto);
                    _objects.Add(entity);
                    resultEntities.Add(entity);
                }
                catch (ArgumentException ex)
                {
                    return Response.Fail(ex.Message);
                }
            }

            return Response.Success(resultEntities, "Tüm noktalar başarıyla eklendi.");
        }

        public object GetAll()
        {
            return Response.Success(_objects, "Tüm noktalar listelendi.");
        }

        public object GetById(int id)
        {
            var entity = _objects.FirstOrDefault(p => p.Id == id);
            if (entity == null)
            {
                return Response.Fail(messages.PointNotFound);
            }

            return Response.Success(entity, "Nokta bulundu.");
        }

        public object Update(int id, ObjectDto dto)
        {
            var entity = _objects.FirstOrDefault(p => p.Id == id);
            if (entity == null)
            {
                return Response.Fail(messages.PointNotFound);
            }

            var errors = ValidateDto(dto);
            if (errors.Any())
            {
                return Response.Fail(messages.InvalidData + ": " + string.Join("; ", errors));
            }
            try
            {
                entity.Name = dto.Name;
                entity.Geometry = _wktReader.Read(dto.Wkt);
                return Response.Success(entity, messages.PointUpdated);
            }
            catch (ArgumentException ex)
            {
                return Response.Fail(ex.Message);
            }
        }

        public object Delete(int id)
        {
            var entity = _objects.FirstOrDefault(p => p.Id == id);
            if (entity == null)
            {
                return Response.Fail(messages.PointNotFound);
            }

            _objects.Remove(entity);
            return Response.Success(true, messages.PointDeleted);
        }
    }
}
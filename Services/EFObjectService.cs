using StajP.DTOs;
using StajP.Interfaces;
using StajP.Properties;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.IO;
using ObjectEntity = StajP.Entities.Object;

namespace StajP.Services
{
    public class EFObjectService : IObjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WKTReader _wktReader = new WKTReader();

        public EFObjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private List<string> ValidateDto(ObjectDto dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);
            return results.Select(r => r.ErrorMessage).ToList();
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
                var geometry = _wktReader.Read(dto.Wkt);
                var entity = new ObjectEntity { Name = dto.Name, Geometry = geometry };

                _unitOfWork.Objects.Add(entity);
                _unitOfWork.Complete();

                return Response.Success(entity, messages.PointCreated);
            }
            catch (Exception ex)
            {
                return Response.Fail("Veritabanı hatası: " + ex.Message);
            }
        }

        public object AddRange(List<ObjectDto> dtos)
        {
            var allErrors = new List<string>();
            var entities = new List<ObjectEntity>();
            foreach (var dto in dtos)
            {
                allErrors.AddRange(ValidateDto(dto));
                if (allErrors.Any()) return Response.Fail(messages.InvalidData + ": " + string.Join("; ", allErrors));

                try
                {
                    var geometry = _wktReader.Read(dto.Wkt);
                    entities.Add(new ObjectEntity { Name = dto.Name, Geometry = geometry });
                }
                catch (Exception ex)
                {
                    return Response.Fail("Geçersiz WKT formatı: " + ex.Message);
                }
            }

            _unitOfWork.Objects.AddRange(entities);
            _unitOfWork.Complete();

            return Response.Success(entities, "Tüm noktalar başarıyla eklendi.");
        }

        public object GetAll()
        {
            var objects = _unitOfWork.Objects.GetAll();
            return Response.Success(objects, "Tüm noktalar listelendi.");
        }

        public object GetById(int id)
        {
            var obj = _unitOfWork.Objects.GetById(id);
            if (obj == null)
            {
                return Response.Fail(messages.PointNotFound);
            }
            return Response.Success(obj, "Nokta bulundu.");
        }

        public object Update(int id, ObjectDto dto)
        {
            var errors = ValidateDto(dto);
            if (errors.Any())
            {
                return Response.Fail(messages.InvalidData + ": " + string.Join("; ", errors));
            }

            var entity = _unitOfWork.Objects.GetById(id);
            if (entity == null)
            {
                return Response.Fail(messages.PointNotFound);
            }

            try
            {
                entity.Name = dto.Name;
                entity.Geometry = _wktReader.Read(dto.Wkt);

                _unitOfWork.Objects.Update(entity);
                _unitOfWork.Complete();

                return Response.Success(entity, messages.PointUpdated);
            }
            catch (Exception ex)
            {
                return Response.Fail("Veritabanı hatası: " + ex.Message);
            }
        }

        public object Delete(int id)
        {
            var entity = _unitOfWork.Objects.GetById(id);
            if (entity == null)
            {
                return Response.Fail(messages.PointNotFound);
            }
            _unitOfWork.Objects.Remove(entity);
            _unitOfWork.Complete();

            return Response.Success(true, messages.PointDeleted);
        }
    }
}
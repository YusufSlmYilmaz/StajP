using Npgsql;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using StajP.DTOs;
using StajP.Interfaces;
using StajP.Properties;
using System.ComponentModel.DataAnnotations;
using ObjectEntity = StajP.Entities.Object;

namespace StajP.Services
{
    public class ADONetObjectService : IObjectService
    {
        private readonly string _connectionString;
        private readonly WKTReader _wktReader = new WKTReader();
        private readonly WKTWriter _wktWriter = new WKTWriter();

        public ADONetObjectService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private List<string> ValidateDto(ObjectDto dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);
            return results.Select(r => r.ErrorMessage).ToList();
        }

        private Geometry ConvertWktToGeometry(string wkt)
        {
            try
            {
                return _wktReader.Read(wkt);
            }
            catch (ParseException ex)
            {
                throw new ArgumentException("WKT formatı geçersiz.", ex);
            }
        }

        private string ConvertGeometryToWkt(Geometry geometry)
        {
            return _wktWriter.Write(geometry);
        }

        public object Add(ObjectDto dto)
        {
            var errors = ValidateDto(dto);
            if (errors.Any())
            {
                return Response.Fail(messages.InvalidData + ": " + string.Join("; ", errors));
            }

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    var geometry = ConvertWktToGeometry(dto.Wkt);
                    var wktLiteral = ConvertGeometryToWkt(geometry);

                    using (var cmd = new NpgsqlCommand("INSERT INTO objects (name, geometry) VALUES (@name, ST_GeomFromText(@wkt, 4326)) RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("name", dto.Name);
                        cmd.Parameters.AddWithValue("wkt", wktLiteral);
                        var id = (int)cmd.ExecuteScalar();

                        var newObject = new ObjectEntity { Id = id, Name = dto.Name, Geometry = geometry };
                        return Response.Success(newObject, messages.PointCreated);
                    }
                }
                catch (ArgumentException ex)
                {
                    return Response.Fail(ex.Message);
                }
                catch (Exception ex)
                {
                    return Response.Fail("Veritabanı hatası: " + ex.Message);
                }
            }
        }

        public object AddRange(List<ObjectDto> dtos)
        {
            var resultEntities = new List<ObjectEntity>();
            var allErrors = new List<string>();

            foreach (var dto in dtos)
            {
                allErrors.AddRange(ValidateDto(dto));
            }

            if (allErrors.Any()) return Response.Fail(messages.InvalidData + ": " + string.Join("; ", allErrors));

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var dto in dtos)
                        {
                            var geometry = ConvertWktToGeometry(dto.Wkt);
                            var wktLiteral = ConvertGeometryToWkt(geometry);

                            using (var cmd = new NpgsqlCommand("INSERT INTO objects (name, geometry) VALUES (@name, ST_GeomFromText(@wkt, 4326)) RETURNING id", conn))
                            {
                                cmd.Parameters.AddWithValue("name", dto.Name);
                                cmd.Parameters.AddWithValue("wkt", wktLiteral);
                                var id = (int)cmd.ExecuteScalar();

                                var newObject = new ObjectEntity { Id = id, Name = dto.Name, Geometry = geometry };
                                resultEntities.Add(newObject);
                            }
                        }
                        transaction.Commit();
                        return Response.Success(resultEntities, "Tüm noktalar başarıyla eklendi.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Response.Fail("Veritabanı hatası: " + ex.Message);
                    }
                }
            }
        }

        public object GetAll()
        {
            var objects = new List<ObjectEntity>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, name, ST_AsText(geometry) as wkt FROM objects", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var wktText = reader.GetString(2);
                            objects.Add(new ObjectEntity
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Geometry = _wktReader.Read(wktText)
                            });
                        }
                    }
                }
            }
            return Response.Success(objects, "Tüm noktalar listelendi.");
        }

        public object GetById(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, name, ST_AsText(geometry) as wkt FROM objects WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var wktText = reader.GetString(2);
                            var obj = new ObjectEntity
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Geometry = _wktReader.Read(wktText)
                            };
                            return Response.Success(obj, "Nokta bulundu.");
                        }
                        else
                        {
                            return Response.Fail(messages.PointNotFound);
                        }
                    }
                }
            }
        }

        public object Update(int id, ObjectDto dto)
        {
            var errors = ValidateDto(dto);
            if (errors.Any())
            {
                return Response.Fail(messages.InvalidData + ": " + string.Join("; ", errors));
            }

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    var geometry = ConvertWktToGeometry(dto.Wkt);
                    var wktLiteral = ConvertGeometryToWkt(geometry);

                    using (var cmd = new NpgsqlCommand("UPDATE objects SET name = @name, geometry = ST_GeomFromText(@wkt, 4326) WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("name", dto.Name);
                        cmd.Parameters.AddWithValue("wkt", wktLiteral);
                        cmd.Parameters.AddWithValue("id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            var updatedObject = new ObjectEntity { Id = id, Name = dto.Name, Geometry = geometry };
                            return Response.Success(updatedObject, messages.PointUpdated);
                        }
                        else
                        {
                            return Response.Fail(messages.PointNotFound);
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    return Response.Fail(ex.Message);
                }
                catch (Exception ex)
                {
                    return Response.Fail("Veritabanı hatası: " + ex.Message);
                }
            }
        }

        public object Delete(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM objects WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Response.Success(true, messages.PointDeleted);
                        }
                        else
                        {
                            return Response.Fail(messages.PointNotFound);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Response.Fail("Veritabanı hatası: " + ex.Message);
                }
            }
        }
    }
}
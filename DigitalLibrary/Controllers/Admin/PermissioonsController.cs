using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using DigitalLibrary.DTOs;
using DigitalLibrary.Repositories;
using Microsoft.VisualBasic;
using DigitalLibrary.DTOs.User;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using DigitalLibrary.Services;
using DigitalLibrary.DTOs.Roles;
using System.Runtime.InteropServices;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize(Roles = "Admin")]
    public class PermissioonsController : ControllerBase
    {
        private readonly IPermissionRepository _repo;
        public PermissioonsController(IPermissionRepository repo)
        {
            _repo = repo;
            
        }
        
        // GET: api/Users
        [HttpGet]
        
        public async Task<ActionResult<ApiResponse<ICollection<PermissionDto>>>> GetPermissions()
        {
            var results = await _repo.GetAllAsync();
            var profiles = new List<PermissionDto>();
            foreach (var result in results) {
                profiles.Add(new PermissionDto { Name = result.Name });
            }
            return Ok( new ApiResponse<ICollection<PermissionDto>>
            {
                Success = true,
                Message = "Lấy danh sách permission thành công",
                Data = profiles
            });
        }

 
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PermissionDto>>> Update(string id, PermissionDto dto)
        {
            var update = await _repo.Find(id);
            if (update == null)
            {
                return NotFound(new ApiResponse<PermissionDto>
                {
                    Success = false,
                    Message = "Không tìm thấy permission",
                });
            }
            update.Name = dto.Name;
            await this._repo.Update(update);
            var response = new PermissionDto
            {
                Name = update.Name,
            };
            return Ok(new ApiResponse<PermissionDto>
            {
                Success = true,
                Message = "Cập nhật thành công",
                Data = response
            });
        }
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PermissionDto>>> Add([FromBody] PermissionDto dto)
        {
            var maxId = await _repo.GetMaxId();
            var role = new Permission
            {
                ID = (maxId + 1).ToString(),
                Name = dto.Name,
            };
            string id = role.ID;
            await this._repo.Add(role);
            var response = new PermissionDto
            {
                Name = role.Name
            };
            return Ok(new ApiResponse<PermissionDto>
            {
                Success = true,
                Message = "Thêm thành công",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<PermissionDto>>> Delete(string id)
        {
            var result = await _repo.Find(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<PermissionDto>
                {
                    Success = false,
                    Message = "Không tìm thấy permission",
                });
            }
            var response = new PermissionDto
            {
              Name = result.Name
            };
            try
            {
                await this._repo.Delete(result);
                return Ok(new ApiResponse<PermissionDto>
                {
                    Success = true,
                    Message = "Xóa thành công",
                    Data = response
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new ApiResponse<PermissionDto>
                {
                    Success = false,
                    Message = "Không thể xóa permission vì còn dữ liệu liên quan",
                    Data = response
                });
            } 
        }
    }
}

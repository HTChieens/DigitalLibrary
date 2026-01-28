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
using Microsoft.EntityFrameworkCore.Metadata;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;

        public RolesController(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }
        
        // GET: api/Users
        [HttpGet]
        
        public async Task<ActionResult<ApiResponse<ICollection<RoleProfileDto>>>> GetRoles()
        {
            var roles = await _roleRepo.GetAllAsync();
            var profiles = new List<RoleProfileDto>();
            foreach (var role in roles) {
                profiles.Add(new RoleProfileDto { Name = role.Name });
            }
            return Ok( new ApiResponse<ICollection<RoleProfileDto>>
            {
                Success = true,
                Message = "Lấy danh sách role thành công",
                Data = profiles
            });
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleProfileDto>>> GetRole(string  id)
        {
            var role= await _roleRepo.Find(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy role lỗi",
                });
            }
            return Ok(new ApiResponse<RoleProfileDto>
            {
                Success = true,
                Message = "Lấy role thành công",
                Data = new RoleProfileDto { Name = role.Name }
            });
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RoleDto>>> Update(string id, RoleDto dto)
        {
            var roleUpdate = await _roleRepo.Find(id);
            if (roleUpdate == null)
            {
                return NotFound(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Không tìm thấy role",
                });
            }
            roleUpdate.Name = dto.Name;
            await this._roleRepo.Update(roleUpdate);
            var response = new RoleDto
            {
                Name = roleUpdate.Name,
            };
            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Cập nhật thành công",
                Data = response
            });
        }
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoleDto>>> Add([FromBody] RoleDto dto)
        {
            var maxId = await _roleRepo.GetMaxId();
            var role = new Role
            {
                ID = (maxId + 1).ToString(),
                Name = dto.Name,
            };
            string id = role.ID;
            await this._roleRepo.Add(role);
            var response = new RoleDto
            {
                Name = role.Name
            };
            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Thêm thành công",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<RoleDto>>> Delete(string id)
        {
            var role = await _roleRepo.Find(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Không tìm thấy role",
                });
            }
            var response = new RoleDto
            {
              Name = role.Name
            };
            try
            {
                await this._roleRepo.Delete(role);
                return Ok(new ApiResponse<RoleDto>
                {
                    Success = true,
                    Message = "Xóa thành công",
                    Data = response
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Không thể xóa role vì còn dữ liệu liên quan",
                    Data = response
                });
            } 
        }
    }
}

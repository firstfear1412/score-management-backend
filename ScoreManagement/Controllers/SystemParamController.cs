using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreManagement.Controllers
{
    //[AllowAnonymous]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SystemParamController : BaseController
    {
        private readonly ISystemParamQuery _systemParamQuery;

        public SystemParamController(ISystemParamQuery systemParamQuery)
        {
            _systemParamQuery = systemParamQuery;
        }

        [HttpGet("GetSystemParam")]
        public async Task<IActionResult> GetSystemParam()
        {
            try
            {
                // Get system parameters
                var systemParams = await _systemParamQuery.GetSysbyteDesc();

                if (systemParams == null || systemParams.Count == 0)
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = "No data exists in this table or view."
                    });
                }

                // Transform data into the desired format
                var result = systemParams
                    .GroupBy(sp => sp.byte_reference)
                    .Select(group => new MasterData
                    {
                        byte_reference = group.Key,
                        byteDetail = group.Select(sp => new ByteDetail
                        {
                            byte_code = sp.byte_code,
                            byte_desc_th = sp.byte_desc_th,
                            byte_desc_en = sp.byte_desc_en,
                            create_date = sp.create_date,
                            active_status = sp.active_status
                        }).ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    isSuccess = true,
                    data = new { masterData = result }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the request.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("UpdateSystemParam")]
        public async Task<IActionResult> UpdateSystemParam([FromBody] SystemParamResource updateRequest)
        {
            try
            {
                if (updateRequest == null || string.IsNullOrEmpty(updateRequest.byte_code) || string.IsNullOrEmpty(updateRequest.byte_reference))
                {
                    return BadRequest(new { isSuccess = false, message = "Invalid request data. byte_code and byte_reference are required." });
                }

                bool isUpdated = await _systemParamQuery.UpdateSystemParam(updateRequest);

                //if (!isUpdated)
                //{
                //    return BadRequest(new { isSuccess = false, message = "Duplicate description found." });
                //}

                return Ok(new { isSuccess = true, message = "Record updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the record.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("InsertSystemParam")]
        public async Task<IActionResult> InsertSystemParam([FromBody] SystemParamResource param)
        {
            try
            {
                var result = await _systemParamQuery.InsertSystemParam(param);

                if (!result.IsSuccess)
                {
                    return BadRequest(new
                    {
                        isSuccess = result.IsSuccess,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    isSuccess = result.IsSuccess,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = "An error occurred while processing the request.",
                    error = ex.Message
                });
            }
        }


    }
}
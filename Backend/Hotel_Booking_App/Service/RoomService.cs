using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Booking_App.Interface.Hotel_Room;
using Hotel_Booking_App.Models;
using Hotel_Booking_App.Models.DTOs.Hotel_Room;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;

namespace Hotel_Booking_App.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public RoomService(IRoomRepository roomRepository, IHotelRepository hotelRepository, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

      
        public async Task<RoomResponseDto> AddRoomAsync(int ownerId, RoomRequestDto dto)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(dto.HotelId);
            if (hotel == null)
            {
                throw new KeyNotFoundException("Hotel not found.");
            }
            if (hotel.HotelOwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("You can only add rooms to your own hotels.");
            }

            var room = _mapper.Map<Room>(dto);
            room.HotelId = hotel.Id;  

            await _roomRepository.AddRoomAsync(room);

            return _mapper.Map<RoomResponseDto>(room);
        }

        
        public async Task<IEnumerable<RoomResponseDto>> GetRoomsByHotelIdAsync(int hotelId)
        {
            var rooms = await _roomRepository.GetRoomsByHotelIdAsync(hotelId);
            return _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);
        }

       
        public async Task<RoomResponseDto> UpdateRoomAsync(int ownerId, int roomId, JsonPatchDocument<RoomRequestDto> patchDto)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
                throw new KeyNotFoundException("Room not found.");

            if (room.Hotel == null || room.Hotel.HotelOwnerId != ownerId)
                throw new UnauthorizedAccessException("You are not authorized to update this room.");

            var roomDto = _mapper.Map<RoomRequestDto>(room);
            patchDto.ApplyTo(roomDto);

            _mapper.Map(roomDto, room);
            await _roomRepository.SaveChangesAsync();

            return _mapper.Map<RoomResponseDto>(room);
        }





        
        public async Task<bool> DeleteRoomAsync(int ownerId, int roomId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return false; 
            }

            var hotel = await _hotelRepository.GetHotelByIdAsync(room.HotelId);
            if (hotel == null || hotel.HotelOwnerId != ownerId)
            {
                return false; 
            }

            await _roomRepository.DeleteRoomAsync(room);
            return true; 
        }
    }
}

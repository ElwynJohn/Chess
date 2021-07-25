cmake_minimum_required(VERSION 3.10)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE "Debug" CACHE STRING "" FORCE)
endif()

project(ChessEngine C)

add_subdirectory(ChessEngine)

if(CMAKE_BUILD_TYPE MATCHES "Rel")
  set(DOTNET_BUILD_TYPE Release)
else()
  set(DOTNET_BUILD_TYPE Debug)
endif()

message("Dotnet build type: " ${DOTNET_BUILD_TYPE})

add_custom_command(
  OUTPUT ChessGUI
  COMMAND ${CMAKE_COMMAND} -E copy_directory
  ${CMAKE_CURRENT_LIST_DIR}/Chess/Assets ${CMAKE_BINARY_DIR}/Assets
  COMMAND dotnet build ${CMAKE_CURRENT_LIST_DIR} -c ${DOTNET_BUILD_TYPE} -o ${CMAKE_BINARY_DIR}
  )
add_custom_target(gui ALL
  DEPENDS ChessGUI
  )

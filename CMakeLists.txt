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

foreach(file COPYING COPYING.LIB)
  if(NOT EXISTS ${CMAKE_BINARY_DIR}/pthreads-win32-${file})
    message("Downloading pthreads-win32 " ${file})
    file(DOWNLOAD
      https://mirrors.kernel.org/sourceware/pthreads-win32/${file}
      ${CMAKE_BINARY_DIR}/pthreads-win32-${file}
      )
  endif()
endforeach()

if(WIN32)
  # Need to use find_program since pthread.dll will be on PATH
  find_program(PTHREAD_DLL_PATH pthread.dll)
endif()

add_custom_command(
  OUTPUT ChessPublish
  COMMAND dotnet publish ${CMAKE_CURRENT_LIST_DIR}/Chess/Chess.csproj -o ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy_directory
  ${CMAKE_CURRENT_LIST_DIR}/Chess/Assets ${CMAKE_BINARY_DIR}/publish/Assets
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/pthreads-win32-COPYING ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/pthreads-win32-COPYING.lib ${CMAKE_BINARY_DIR}/publish
  # TODO: Would be nice if there was a way to resolve a target's dependencies
  # automatically
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/ChessEngineMain.exe ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/ChessEngine.dll ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/rgl.dll ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/ipc.dll ${CMAKE_BINARY_DIR}/publish
  DEPENDS ChessCopyPthreads.dll
  )

# TODO: This is kinda ugly, it's awkward because Windows needs to copy
# pthread.dll into the publish dir but *nix does not
if(WIN32)
  add_custom_command(
    OUTPUT ChessCopyPthreads.dll
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy ${PTHREAD_DLL_PATH} ${CMAKE_BINARY_DIR}/publish/
    )
else()
  add_custom_command(
    OUTPUT ChessCopyPthreads.dll
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_BINARY_DIR}/publish/
    )
endif()

add_custom_target(publish DEPENDS ChessPublish)

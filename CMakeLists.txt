cmake_minimum_required(VERSION 3.10)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE "Debug" CACHE STRING "" FORCE)
endif()

# This makes macOS find dylibs when we run?
set(CMAKE_SKIP_BUILD_RPATH FALSE)
set(CMAKE_BUILD_WITH_INSTALL_RPATH FALSE)

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

if(WIN32)
  foreach(file COPYING COPYING.LIB)
    if(NOT EXISTS ${CMAKE_BINARY_DIR}/pthreads-win32-${file})
      message("Downloading pthreads-win32 " ${file})
      file(DOWNLOAD
        https://mirrors.kernel.org/sourceware/pthreads-win32/${file}
        ${CMAKE_BINARY_DIR}/pthreads-win32-${file}
        )
    endif()
  endforeach()
endif()

if(WIN32)
  # Need to use find_program since pthread.dll will be on PATH
  find_program(PTHREAD_DLL_PATH pthread.dll)
endif()

set(ChessPublishDepends ChessEngine ChessEngineMain)

if(WIN32)
  list(APPEND ChessPublishDepends ChessCopyPthreads.dll ChessBundleCdlls)
  add_custom_command(
    OUTPUT ChessCopyPthreads.dll
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy ${PTHREAD_DLL_PATH} ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/pthreads-win32-COPYING ${CMAKE_BINARY_DIR}/publish
    COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/pthreads-win32-COPYING.lib ${CMAKE_BINARY_DIR}/publish
    )
  add_custom_command(
    OUTPUT ChessBundleCdlls
    # NOTE: This requires VC++ 2015 installed system wide (for developers)
    COMMAND ${CMAKE_COMMAND} -E copy /Windows/System32/ucrtbase.dll ${CMAKE_BINARY_DIR}/publish/
    COMMAND ${CMAKE_COMMAND} -E copy /Windows/System32/vcruntime140.dll ${CMAKE_BINARY_DIR}/publish/
    )
endif()

if(WIN32)
  set(ExeExt .exe)
  set(DynamicLibExt dll)
  set(DotnetRID win-x64)
elseif(APPLE)
  set(DynamicLibExt dylib)
  set(DynamicLibPre lib)
  set(DotnetRID osx-x64)
elseif(UNIX)
  set(DynamicLibExt so)
  set(DynamicLibPre lib)
  set(DotnetRID linux-x64)
endif()

add_custom_command(
  OUTPUT ChessDotnetPublish
  COMMAND dotnet publish ${CMAKE_CURRENT_LIST_DIR}/Chess/Chess.csproj -o
  ${CMAKE_BINARY_DIR}/publish -c Release -r ${DotnetRID}
  COMMAND ${CMAKE_COMMAND} -E copy_directory
  ${CMAKE_CURRENT_LIST_DIR}/Chess/Assets ${CMAKE_BINARY_DIR}/publish/Assets
  )

add_custom_command(
  OUTPUT ChessPublish
  # TODO: Would be nice if there was a way to resolve a target's dependencies
  # automatically
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/ChessEngineMain${ExeExt} ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy
  ${CMAKE_BINARY_DIR}/${DynamicLibPre}ChessEngine.${DynamicLibExt} ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/${DynamicLibPre}rgl.${DynamicLibExt} ${CMAKE_BINARY_DIR}/publish
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/${DynamicLibPre}ipc.${DynamicLibExt} ${CMAKE_BINARY_DIR}/publish
  DEPENDS ${ChessPublishDepends} ChessDotnetPublish
  )

add_custom_target(publish DEPENDS ChessPublish)

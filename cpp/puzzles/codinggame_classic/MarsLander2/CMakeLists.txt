# Executable name definition

set(EXECUTABLE_NAME CodingGame_classic_MarsLander2)

# Header search directories

include_directories(${SDL2_INCLUDE_DIR})

# Executable creation

add_executable(
    ${EXECUTABLE_NAME}
    main.cpp
)

# Libraries dependencies

target_link_libraries(
    ${EXECUTABLE_NAME}
    ${SDL2_LIBRARY}
)

# Copy dependant binaries

#configure_file(${CMAKE_CURRENT_SOURCE_DIR} ${CMAKE_CURRENT_BINARY_DIR} @ONLY)

# Definitions

add_definitions("-DLOCAL")
# Executable name definition

set(EXECUTABLE_NAME BattleDev_Nov2017_1)

# Executable creation

add_executable(
    ${EXECUTABLE_NAME}
    main.cpp
)

# Include directories

include_directories(../../../Validation)

# Dependencies

add_dependencies(
    ${EXECUTABLE_NAME}
    Validation
)

target_link_libraries(
    ${EXECUTABLE_NAME}
    Validation
)

# Copy data

configure_files(
    ${CMAKE_CURRENT_SOURCE_DIR}/data
    ${CMAKE_CURRENT_BINARY_DIR}/data
)